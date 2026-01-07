using System.ComponentModel.DataAnnotations;
using BookVerse.Core.Interfaces;

namespace BookVerse.Core.Entities;

public class Author : IAuditable, IEntity
{
    [Key] public int Id { get; set; }

    [Required] [MaxLength(100)] public string FirstName { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    // Navigation
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}