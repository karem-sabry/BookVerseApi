using AutoMapper;
using BookVerse.Application.Dtos.Book;
using BookVerse.Application.Interfaces;
using BookVerse.Core.Entities;
using BookVerse.Core.Models;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class BooksService : IBooksService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public BooksService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<PagedResult<BookReadDto>> GetPagedAsync(BookQueryParameters parameters)
    {
        var pagedBooks = await _unitOfWork.Books.GetPagedWithDetailsAsync(parameters);
        var bookDtos = _mapper.Map<IEnumerable<BookReadDto>>(pagedBooks.Items);

        return new PagedResult<BookReadDto>(
            bookDtos,
            pagedBooks.TotalCount,
            pagedBooks.CurrentPage,
            pagedBooks.PageSize
        );
    }

    public async Task<IEnumerable<BookReadDto>> GetAllAsync()
    {
        var books = await _unitOfWork.Books.GetAllAsync();
        return _mapper.Map<IEnumerable<BookReadDto>>(books);
    }

    public async Task<BookReadDto?> GetByIdAsync(int id)
    {
        var book = await _unitOfWork.Books.GetByIdWithDetailsAsync(id);
        return book == null ? null : _mapper.Map<BookReadDto>(book);
    }

    public async Task<BookReadDto> CreateAsync(BookCreateDto bookDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var book = _mapper.Map<Book>(bookDto);
            var existingBook = await _unitOfWork.Books.GetExistingBook(book);

            if (existingBook != null)
            {
                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<BookReadDto>(existingBook);
            }

            await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();

            // add relationships
            foreach (var authorId in bookDto.AuthorIds)
            {
                var bookAuthor = new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.BookAuthors.AddAsync(bookAuthor);
            }

            foreach (var categoryId in bookDto.CategoryIds)
            {
                var bookCategory = new BookCategory
                {
                    BookId = book.Id,
                    CategoryId = categoryId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.BookCategories.AddAsync(bookCategory);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var createdBook = await _unitOfWork.Books.GetByIdWithDetailsAsync(book.Id);
            return _mapper.Map<BookReadDto>(createdBook!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, BookUpdateDto bookDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var book = await _unitOfWork.Books.GetByIdWithDetailsAsync(id);
            if (book == null) return false;

            //Update basic properties
            _mapper.Map(bookDto, book);
            _unitOfWork.Books.Update(book);

            //Remove existing author relationships
            var existingAuthorRelations = await _context.BookAuthors
                .Where(ba => ba.BookId == id)
                .ToListAsync();
            _context.BookAuthors.RemoveRange(existingAuthorRelations);

            //Add new author relationships
            foreach (var authorId in bookDto.AuthorIds)
            {
                var bookAuthor = new BookAuthor
                {
                    BookId = id,
                    AuthorId = authorId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.BookAuthors.AddAsync(bookAuthor);
            }

            //Remove existing category relationships
            var existingCategoryRelations = await _context.BookCategories.Where(bc => book.Id == id)
                .ToListAsync();

            _context.BookCategories.RemoveRange(existingCategoryRelations);

            //Add new category relationships
            foreach (var categoryId in bookDto.CategoryIds)
            {
                var bookCategory = new BookCategory
                {
                    BookId = id,
                    CategoryId = categoryId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.BookCategories.AddAsync(bookCategory);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _unitOfWork.Books.GetByIdWithDetailsAsync(id);
        if (book == null) return false;

        _unitOfWork.Books.Delete(book);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}