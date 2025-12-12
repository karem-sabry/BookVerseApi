using BookVerse.Application.Interfaces;
using BookVerse.Core.Entities;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>,IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet.AsNoTracking().Include(o => o.OrderItems).ThenInclude(oi => oi.Book).FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
    {
        return await _dbSet.Include(o => o.OrderItems).ThenInclude(oi => oi.Book).Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc).ToListAsync();
    }

    public async Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId)
    {
        return await _dbSet.Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntentId);
    }
}