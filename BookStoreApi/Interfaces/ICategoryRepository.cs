using BookStoreApi.Entities;

namespace BookStoreApi.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByNameAsync(string Name);
    Task AddAsync(Category category);
    void Update(Category category);
    void Delete(Category category);
    Task SaveAsync();
}