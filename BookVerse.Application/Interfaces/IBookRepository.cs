using BookVerse.Core.Entities;
using BookVerse.Core.Models;

namespace BookVerse.Application.Interfaces;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<Book?> GetByIdWithDetailsAsync(int id);
    Task<PagedResult<Book>> GetPagedWithDetailsAsync(BookQueryParameters parameters);
    Task<Book?> GetExistingBook(Book book);
}