using BookVerse.Application.Dtos.Order;

namespace BookVerse.Application.Interfaces;

public interface IOrderService
{
    Task<OrderReadDto> CreateOrderAsync(OrderCreateDto orderDto, Guid userId);
    Task<PaymentIntentDto> CreatePaymentIntentAsync(int orderId, Guid userId);
    Task<OrderReadDto?> GetOrderByIdAsync(int orderId, Guid userId);
    Task<IEnumerable<OrderReadDto>> GetUserOrdersAsync(Guid userId);
    Task<bool> UpdateOrderPaymentStatusAsync(string paymentIntentId, string status);
}