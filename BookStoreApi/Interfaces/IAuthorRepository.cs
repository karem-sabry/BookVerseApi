using BookStoreApi.Entities;

namespace BookStoreApi.Interfaces;

public interface IAuthorRepository
{
    Task<IEnumerable<Author>> GetAllAsync();
    Task<Author?> GetByIdAsync(int id);
    Task<Author?> GetByNameAsync(string FirstName,string lastName);
    
    Task AddAsync(Author author);
    void Update(Author author);
    void Delete(Author author);
    Task SaveAsync();
}