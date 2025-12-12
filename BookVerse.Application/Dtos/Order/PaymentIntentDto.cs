namespace BookVerse.Application.Dtos.Order;

public class PaymentIntentDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public int OrderId { get; set; }
}