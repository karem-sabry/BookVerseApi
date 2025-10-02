using BookStoreApi.Entities;

namespace BookStoreApi.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<Book?> GetExistingBook(Book book);
    
    Task AddAsync(Book book);
    void Update(Book book);
    void Delete(Book book);
    Task SaveAsync();
}