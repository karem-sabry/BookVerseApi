using BookVerse.Application.Dtos.Book;
using BookVerse.Application.Interfaces;
using BookVerse.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookController : ControllerBase
{
    private readonly IBooksService _booksService;

    public BookController(IBooksService booksService)
    {
        _booksService = booksService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<BookReadDto>),StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BookReadDto>>> GetBooks([FromQuery] BookQueryParameters parameters)
    {
        var books = await _booksService.GetPagedAsync(parameters);
        return Ok(books);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BookReadDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookReadDto>> GetBookById(int id)
    {
        var book = await _booksService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return Ok(book);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BookReadDto),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookReadDto>> CreateBook(BookCreateDto bookDto)
    {
        var createdBook = await _booksService.CreateAsync(bookDto);
        return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBook(int id, BookUpdateDto bookDto)
    {
        return await _booksService.UpdateAsync(id, bookDto) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        return await _booksService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}