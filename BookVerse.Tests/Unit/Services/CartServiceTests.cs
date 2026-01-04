using AutoMapper;
using BookVerse.Application.Dtos.Cart;
using BookVerse.Application.Interfaces;
using BookVerse.Core.Entities;
using BookVerse.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookVerse.Tests.Unit.Services;

public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CartService>> _mockLogger;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly CartService _sut;
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<IBookRepository> _mockBookRepository;

    public CartServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CartService>>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _mockCartRepository = new Mock<ICartRepository>();
        _mockBookRepository = new Mock<IBookRepository>();

        _mockDateTimeProvider.Setup(x => x.UtcNow).Returns(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        _mockUnitOfWork.Setup(x => x.Carts).Returns(_mockCartRepository.Object);
        _mockUnitOfWork.Setup(x => x.Books).Returns(_mockBookRepository.Object);
        
        _sut = new CartService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockDateTimeProvider.Object);
    }

    #region GetCartByUserIdAsync Tests

    [Fact]
    public async Task GetCartByUserIdAsync_WithExistingCart_ReturnsCartDto()
    {
        // Arrange
        
        var userId = Guid.NewGuid();
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Price = 29.99m,
            QuantityInStock = 10,
            PublishDate = new DateOnly(2024, 1, 1)
        };

        var cart = new Cart
        {
            Id = 1,
            UserId = userId,
            CreatedAtUtc = _mockDateTimeProvider.Object.UtcNow,
            CartItems = new List<CartItem>
            {
                new()
                {
                    Id = 1,
                    CartId = 1,
                    BookId = 1,
                    Book = book,
                    Quantity = 2,
                    PriceAtAdd = 29.99m
                }
            }
        };
        var cartDto = new CartDto
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItemDto>
            {
                new()
                {
                    Id = 1,
                    BookId = 1,
                    BookTitle = book.Title,
                    Price = book.Price,
                    Quantity = 2
                }
            },

        };
        _mockUnitOfWork.Setup(x => x.Carts.GetUserCartAsync(userId)).ReturnsAsync(cart);
        _mockMapper.Setup(x => x.Map<CartDto>(cart)).Returns(cartDto);
        
        // Act
        var result = await _sut.GetCartByUserIdAsync(userId);


        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.UserId.Should().Be(userId);
        result.CartItems.Should().HaveCount(1);
        result.TotalItems.Should().Be(2);
        result.TotalAmount.Should().Be(59.98m); 
        
        _mockCartRepository.Verify(x=>x.GetUserCartAsync(userId),Times.Once);
        
        _mockMapper.Verify(x=>x.Map<CartDto>(cart),Times.Once());
    }

    [Fact]
    public async Task GetUserCartAsync_WithNonexistentCart_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        _mockUnitOfWork.Setup(x => x.Carts.GetUserCartAsync(userId)).ReturnsAsync((Cart?)null);
        
        // Act
        var result = await _sut.GetCartByUserIdAsync(userId);
        
        // Assert
        result.Should().BeNull();
        _mockCartRepository.Verify(x=>x.GetUserCartAsync(userId),Times.Once());
        _mockMapper.Verify(x=>x.Map<CartDto>(It.IsAny<Cart>()),Times.Never());
    }   
    #endregion

    #region AddItemToCartAsync Tests
    [Fact]
    public async Task AddToCartAsync_WithExistingCart_AddsItemToCart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartItemAdd = new CartItemAdd
        {
            BookId = 1,
            Quantity = 2
        };

        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Price = 19.99m,
            QuantityInStock = 10
        };

        var existingCart = new Cart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>()
        };

        var updatedCart = new Cart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new CartItem
                {
                    Id = 1,
                    CartId = 1,
                    BookId = 1,
                    Quantity = 2,
                    PriceAtAdd = 19.99m,
                    Book = book
                }
            }
        };

        var cartDto = new CartDto
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItemDto>
            {
                new CartItemDto
                {
                    Id = 1,
                    BookId = 1,
                    BookTitle = "Test Book",
                    Price = 19.99m,
                    Quantity = 2
                }
            }
        };

        _mockCartRepository.Setup(x => x.GetUserCartAsync(userId)).ReturnsAsync(existingCart);
        _mockBookRepository.Setup(x => x.GetByIdAsync(cartItemAdd.BookId)).ReturnsAsync(book);
        _mockCartRepository.Setup(x => x.GetCartItemAsync(existingCart.Id, cartItemAdd.BookId))
            .ReturnsAsync((CartItem?)null);
        _mockCartRepository.Setup(x => x.AddCartItemAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
        _mockCartRepository.Setup(x => x.GetCartWithItemsAsync(existingCart.Id)).ReturnsAsync(updatedCart);
        _mockMapper.Setup(x => x.Map<CartDto>(updatedCart)).Returns(cartDto);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddToCartAsync(userId, cartItemAdd);

        // Assert
        result.Should().NotBeNull();
        result.CartItems.Should().HaveCount(1);

        _mockCartRepository.Verify(x => x.AddAsync(It.IsAny<Cart>()), Times.Never);
        _mockCartRepository.Verify(x => x.AddCartItemAsync(It.IsAny<CartItem>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    
    [Fact]
    public async Task AddItemToCartAsync_WithExistingItem_IncreasesQuantity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemDto = new CartItemAdd
        {
            BookId = 1,
            Quantity = 2
        };

        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Price = 20m,
            QuantityInStock = 10,
            PublishDate = new DateOnly(2024, 1, 1)
        };

        var existingCartItem = new CartItem
        {
            Id = 1,
            CartId = 1,
            BookId = 1,
            Book = book,
            Quantity = 3,
            PriceAtAdd = 20m
        };

        var cart = new Cart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem> { existingCartItem }
        };
        
        _mockUnitOfWork.Setup(x => x.Carts.GetUserCartAsync(userId))
            .ReturnsAsync(cart);
        
        _mockUnitOfWork.Setup(x => x.Books.GetByIdAsync(1))
            .ReturnsAsync(book);
        _mockUnitOfWork.Setup(x => x.Carts.GetCartItemAsync(cart.Id, book.Id)).ReturnsAsync(existingCartItem);

        _mockMapper.Setup(x => x.Map<CartDto>(cart)).Returns(new CartDto
        {
            Id = cart.Id,
            CartItems = new List<CartItemDto>
            {
                new()
                {
                    Id = cart.Id,
                    BookId = book.Id,
                    BookTitle = book.Title,
                    Price = book.Price,
                    Quantity = 5
                }
            }
        });
        
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);
        
        // Act
        var result = await _sut.AddToCartAsync(userId, itemDto);

        // Assert
        existingCartItem.Quantity.Should().Be(5); // 3 + 2
        _mockUnitOfWork.Verify(x => x.Carts.UpdateCartItem(existingCartItem), Times.Once);
    }
    #endregion
    #region UpdateCartItemAsync Tests

    [Fact]
    public async Task UpdateCartItemAsync_WithValidData_UpdatesCartItem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartItemId = 1;
        var cartItemUpdate = new CartItemUpdate
        {
            Quantity = 5
        };

        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Price = 19.99m,
            QuantityInStock = 10
        };

        var cartItem = new CartItem
        {
            Id = cartItemId,
            CartId = 1,
            BookId = 1,
            Quantity = 2,
            PriceAtAdd = 19.99m,
            Book = book
        };

        var cart = new Cart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem> { cartItem }
        };

        var updatedCart = new Cart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new CartItem
                {
                    Id = cartItemId,
                    CartId = 1,
                    BookId = 1,
                    Quantity = 5,
                    PriceAtAdd = 19.99m,
                    Book = book
                }
            }
        };

        var cartDto = new CartDto
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItemDto>
            {
                new CartItemDto
                {
                    Id = cartItemId,
                    BookId = 1,
                    BookTitle = "Test Book",
                    Price = 19.99m,
                    Quantity = 5
                }
            }
        };

        _mockCartRepository.Setup(x => x.GetUserCartAsync(userId)).ReturnsAsync(cart);
        _mockBookRepository.Setup(x => x.GetByIdAsync(cartItem.BookId)).ReturnsAsync(book);
        _mockCartRepository.Setup(x => x.GetCartWithItemsAsync(cart.Id)).ReturnsAsync(updatedCart);
        _mockMapper.Setup(x => x.Map<CartDto>(updatedCart)).Returns(cartDto);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateCartItemAsync(userId, cartItemId, cartItemUpdate);

        // Assert
        result.Should().NotBeNull();
        result!.CartItems[0].Quantity.Should().Be(5);

        _mockCartRepository.Verify(x => x.UpdateCartItem(cartItem), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
    #endregion
}










