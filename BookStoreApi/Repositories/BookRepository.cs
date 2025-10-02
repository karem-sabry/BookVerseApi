using BookStoreApi.Data;
using BookStoreApi.Entities;
using BookStoreApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _context.Books.AsNoTracking().Include(b=>b.Author).Include(b=>b.Category).ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books.AsNoTracking().Include(b => b.Author).Include(b=>b.Category).FirstOrDefaultAsync(b => b.Id==id);
    }

    public async Task<Book?> GetExistingBook(Book book)
    {
        var retrievedBook = await _context.Books.Include(b=>b.Author).Include(b=>b.Category).FirstOrDefaultAsync(b => b.Title == book.Title);
        if (retrievedBook != null)
        {
            if (retrievedBook.AuthorId == book.AuthorId && retrievedBook.CategoryId == book.CategoryId)
            {
                return retrievedBook;
            }
            
        }

        return null;
    }


    public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
    }

    public void Update(Book book)
    {
        _context.Books.Update(book);
    }

    public void Delete(Book book)
    {
        _context.Books.Remove(book);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}