using AutoMapper;
using BookStoreApi.Dtos;
using BookStoreApi.Dtos.Author;
using BookStoreApi.Dtos.Book;
using BookStoreApi.Dtos.Category;
using BookStoreApi.Entities;

namespace BookStoreApi.Profiles;

public class MappingProfile:Profile
{
    public MappingProfile()
    {
        //Book Mapping
        CreateMap<Book, BookReadDto>().ForMember(dest=>dest.Author,
            opt=>opt.MapFrom(s=>s.Author.FirstName+" " +s.Author.LastName))
            .ForMember(b=>b.Category,opt=>opt.MapFrom(s=>s.Category.Name));
        CreateMap<Book, BookBriefDto>();
        CreateMap<BookCreateDto, Book>();
        CreateMap<BookUpdateDto, Book>();
        
        // Author Mapping
        CreateMap<AuthorCreateDto, Author>();
        CreateMap<Author, AuthorReadDto>().ForMember(dest=>dest.Books,opt=>opt.MapFrom(src=>src.Books));
        CreateMap<AuthorUpdateDto, Author>();
        
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<Category, CategoryReadDto>();
        CreateMap<CategoryUpdateDto, Category>();
    }
}