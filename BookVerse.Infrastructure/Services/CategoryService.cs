using AutoMapper;
using BookVerse.Application.Dtos.Category;
using BookVerse.Application.Interfaces;
using BookVerse.Core.Entities;
using BookVerse.Core.Models;

namespace BookVerse.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<CategoriesReadDto>> GetPagedAsync(QueryParameters parameters)
    {
        var pagedCategories = await _unitOfWork.Categories.GetPagedAsync(parameters);
        var categoryDtos = _mapper.Map<IEnumerable<CategoriesReadDto>>(pagedCategories.Items);

        return new PagedResult<CategoriesReadDto>(
            categoryDtos,
            pagedCategories.TotalCount,
            pagedCategories.CurrentPage,
            pagedCategories.PageSize
        );
    }

    public async Task<IEnumerable<CategoriesReadDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoriesReadDto>>(categories);
    }

    public async Task<CategoryReadDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category == null ? null : _mapper.Map<CategoryReadDto>(category);
    }


    public async Task<CategoryReadDto> CreateAsync(CategoryCreateDto categoryDto)
    {
        var category = _mapper.Map<Category>(categoryDto);
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(category.Name);
        if (existingCategory != null)
        {
            return _mapper.Map<CategoryReadDto>(existingCategory);
        }

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<CategoryReadDto>(category);
    }

    public async Task<bool> UpdateAsync(int id, CategoryUpdateDto categoryDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return false;

        _mapper.Map(categoryDto, category);
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return false;
        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}