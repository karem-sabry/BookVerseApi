using System.ComponentModel.DataAnnotations;
using BookVerse.Core.Interfaces;

namespace BookVerse.Core.Entities;

public class Order : IAuditable,IEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed
    
    [MaxLength(200)]
    public string? StripePaymentIntentId { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}