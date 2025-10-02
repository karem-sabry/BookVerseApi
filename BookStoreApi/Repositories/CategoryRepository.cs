using BookStoreApi.Data;
using BookStoreApi.Entities;
using BookStoreApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Repositories;

public class CategoryRepository:ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.AsNoTracking().Include(c=>c.Books).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.Include(c=>c.Books).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetByNameAsync(string Name)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Name == Name);
    }

    public async Task AddAsync(Category category)
    {
            await _context.Categories.AddAsync(category);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}