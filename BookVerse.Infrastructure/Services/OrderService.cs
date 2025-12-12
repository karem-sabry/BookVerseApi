using AutoMapper;
using BookVerse.Application.Dtos.Order;
using BookVerse.Application.Interfaces;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Order = BookVerse.Core.Entities.Order;

namespace BookVerse.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly string _stripeSecretKey;

    public OrderService(IUnitOfWork unitOfWork,IMapper mapper,ILogger<OrderService> logger,IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _stripeSecretKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey");
        StripeConfiguration.ApiKey = _stripeSecretKey;
    }
    public async Task<OrderReadDto> CreateOrderAsync(OrderCreateDto orderDto, Guid userId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var order = new Order()
            {
                UserId = userId,
                PaymentStatus = "Pending",
                TotalAmount = 0
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            decimal totalAmount = 0;

            foreach (var item in orderDto.Items)
            {
                var book = await _unitOfWork.Books.GetByIdAsync(item.BookId);

                if (book == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception($"Book with Id {item.BookId} not found");
                }

                if (book.QuantityInStock < item.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception($"Not enough stock for book: {book.Title}");
                }

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    BookId = book.Id,
                    Quantity = item.Quantity,
                    Price = book.Price
                };
                
                order.OrderItems.Add(orderItem);
                totalAmount += book.Price * item.Quantity;

                book.QuantityInStock -= item.Quantity;
                _unitOfWork.Books.Update(book);
            }

            order.TotalAmount = totalAmount;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var createdOrder = await _unitOfWork.Orders.GetByIdWithDetailsAsync(order.Id);
            return MapToOrderReadDto(createdOrder!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error creating order");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int orderId, Guid userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId);

            if (order == null || order.UserId != userId)
            {
                throw new Exception("Order not found");
            }

            if (order.PaymentStatus == PaymentConstants.Completed)
            {
                throw new Exception("Order already paid");
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(order.TotalAmount * 100),
                Currency = "aed",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", orderId.ToString() },
                    { "userId", userId.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            order.StripePaymentIntentId = paymentIntent.Id;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentIntentDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                OrderId = orderId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,$"Error creating payment intent for order {orderId}");
            throw;
        }
    }

    public async Task<OrderReadDto?> GetOrderByIdAsync(int orderId, Guid userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId);

            if (order == null || order.UserId != userId)
            {
                return null;
            }

            return MapToOrderReadDto(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<IEnumerable<OrderReadDto>> GetUserOrdersAsync(Guid userId)
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetUserOrdersAsync(userId);
            return orders.Select(MapToOrderReadDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateOrderPaymentStatusAsync(string paymentIntentId, string status)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByPaymentIntentIdAsync(paymentIntentId);

            if (order == null)
            {
                _logger.LogWarning("Order not found for payment intent {PaymentIntentId}", paymentIntentId);
                return false;
            }

            order.PaymentStatus = status;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status for {PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    private OrderReadDto MapToOrderReadDto(Order order)
    {
        return new OrderReadDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.OrderItems.Select(oi => new OrderItemReadDto
            {
                Id = oi.Id,
                BookId = oi.BookId,
                BookTitle = oi.Book.Title,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };
    }
}