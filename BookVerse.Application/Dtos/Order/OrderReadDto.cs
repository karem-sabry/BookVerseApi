namespace BookVerse.Application.Dtos.Order;

public class OrderReadDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemReadDto> Items { get; set; } = new();

}