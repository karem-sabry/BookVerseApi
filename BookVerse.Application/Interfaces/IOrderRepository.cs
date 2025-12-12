using BookVerse.Core.Entities;

namespace BookVerse.Application.Interfaces;

public interface IOrderRepository:IGenericRepository<Order>
{
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
    Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId);
}