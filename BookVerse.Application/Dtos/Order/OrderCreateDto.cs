using System.ComponentModel.DataAnnotations;

namespace BookVerse.Application.Dtos.Order;

public class OrderCreateDto
{
    [Required(ErrorMessage = "At least one item is required")]
    [MinLength(1,ErrorMessage = "At least one item is required")]
    public List<OrderItemCreateDto> Items { get; set; }
}