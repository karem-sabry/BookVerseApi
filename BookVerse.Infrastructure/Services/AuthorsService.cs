using AutoMapper;
using BookVerse.Application.Dtos.Author;
using BookVerse.Application.Interfaces;
using BookVerse.Core.Entities;
using BookVerse.Core.Models;

namespace BookVerse.Infrastructure.Services;

public class AuthorsService : IAuthorsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuthorsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuthorsReadDto>> GetPagedAsync(QueryParameters parameters)
    {
        var pagedAuthors = await _unitOfWork.Authors.GetPagedAsync(parameters);
        var authorDtos = _mapper.Map<IEnumerable<AuthorsReadDto>>(pagedAuthors.Items);

        return new PagedResult<AuthorsReadDto>(
            authorDtos,
            pagedAuthors.TotalCount,
            pagedAuthors.CurrentPage,
            pagedAuthors.PageSize
        );
    }

    public async Task<IEnumerable<AuthorsReadDto>> GetAllAsync()
    {
        var authors = await _unitOfWork.Authors.GetAllAsync();
        return _mapper.Map<IEnumerable<AuthorsReadDto>>(authors);
    }

    public async Task<AuthorReadDto?> GetByIdAsync(int id)
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(id);
        return _mapper.Map<AuthorReadDto>(author);
    }

    public async Task<AuthorReadDto> CreateAsync(AuthorCreateDto authorDto)
    {
        var author = _mapper.Map<Author>(authorDto);
        var existingAuthor = await _unitOfWork.Authors.GetByNameAsync(author.FirstName, author.LastName);

        if (existingAuthor != null)
        {
            return _mapper.Map<AuthorReadDto>(existingAuthor);
        }

        await _unitOfWork.Authors.AddAsync(author);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AuthorReadDto>(author);
    }

    public async Task<bool> UpdateAsync(int id, AuthorUpdateDto authorDto)
    {
        var retrievedAuthor = await _unitOfWork.Authors.GetByIdAsync(id);
        if (retrievedAuthor == null) return false;

        _mapper.Map(authorDto, retrievedAuthor);
        _unitOfWork.Authors.Update(retrievedAuthor);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var retrievedAuthor = await _unitOfWork.Authors.GetByIdAsync(id);
        if (retrievedAuthor == null) return false;

        _unitOfWork.Authors.Delete(retrievedAuthor);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}