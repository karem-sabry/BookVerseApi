using BookVerse.Application.Interfaces;
using BookVerse.Core.Entities;
using BookVerse.Core.Models;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Repositories;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(AppDbContext context) : base(context)
    {
    }

    public async override Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _dbSet
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<PagedResult<Book>> GetPagedWithDetailsAsync(BookQueryParameters parameters)
    {
        IQueryable<Book> query = _dbSet.Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories)
            .ThenInclude(bc => bc.Category);

        // Apply filters
        query = ApplyFilters(query, parameters);

        // Apply Search
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = ApplySearch(query, parameters.SearchTerm);
        }

        //Get Total count
        var totalCount = await query.CountAsync();

        //apply Sorting
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
        }
        else
        {
            //Default Sorting
            query = query.OrderByDescending(b => b.CreatedAtUtc);
        }

        //Apply pagination
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        return new PagedResult<Book>(items, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<Book?> GetExistingBook(Book book)
    {
        return await _dbSet
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Title == book.Title);
    }

    protected override IQueryable<Book> ApplySearch(IQueryable<Book> query, string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return query.Where(b =>
            b.Title.ToLower().Contains(lowerSearchTerm) ||
            (b.Description != null && b.Description.ToLower().Contains(lowerSearchTerm)) ||
            (b.ISBN != null && b.ISBN.ToLower().Contains(lowerSearchTerm)) ||
            b.BookAuthors.Any(ba =>
                ba.Author.FirstName.ToLower().Contains(lowerSearchTerm) ||
                ba.Author.LastName.ToLower().Contains(lowerSearchTerm))
        );
    }

    private IQueryable<Book> ApplyFilters(IQueryable<Book> query, BookQueryParameters parameters)
    {
        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(b => b.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(b => b.Price <= parameters.MaxPrice.Value);
        }

        if (parameters.AuthorId.HasValue)
        {
            query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == parameters.AuthorId.Value));
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == parameters.CategoryId.Value));
        }

        if (parameters.PublishedAfter.HasValue)
        {
            query = query.Where(b => b.PublishDate >= parameters.PublishedAfter.Value);
        }

        if (parameters.PublishedBefore.HasValue)
        {
            query = query.Where(b => b.PublishDate <= parameters.PublishedBefore.Value);
        }

        return query;
    }
}