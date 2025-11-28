using BookVerse.Application.Dtos.Author;
using BookVerse.Core.Models;

namespace BookVerse.Application.Interfaces;

public interface IAuthorsService
{
    Task<PagedResult<AuthorsReadDto>> GetPagedAsync(QueryParameters parameters);
    Task<IEnumerable<AuthorsReadDto>> GetAllAsync();
    Task<AuthorReadDto?> GetByIdAsync(int id);
    Task<AuthorReadDto> CreateAsync(AuthorCreateDto authorDto);
    Task<bool> UpdateAsync(int id, AuthorUpdateDto authorDto);
    Task<bool> DeleteAsync(int id);
}