using System.Linq.Expressions;
using BookVerse.Core.Models;
namespace BookVerse.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<PagedResult<T>> GetPagedAsync(QueryParameters parameters);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    void Update(T entity);
    void Delete(T entity);
}