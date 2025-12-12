namespace BookVerse.Application.Dtos.Order;

public class OrderItemReadDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}