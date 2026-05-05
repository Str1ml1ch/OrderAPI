using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.DAL;
using OrderAPI.DAL.Entities;
using OrderAPI.DAL.Storage.GetOrderById;
using OrderAPI.Domain.Storage.GetOrderById;

namespace OrderAPI.Tests.DAL;

public class GetOrderByIdStorageTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly GetOrderByIdStorage _sut;

    public GetOrderByIdStorageTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new OrderDbContext(options);
        _sut = new GetOrderByIdStorage(_context);
    }

    public void Dispose() => _context.Dispose();

    private Order CreateOrder(Guid? id = null, EOrderStatus status = EOrderStatus.Pending)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = Homework.Ticketing.System.Shared.Enums.ECurrency.USD,
            OrderStatus = status,
            CreatedAt = DateTimeOffset.UtcNow
        };

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        var result = await _sut.GetOrderByIdAsync(Guid.NewGuid(), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsOrderDetail_WhenOrderExists()
    {
        var order = CreateOrder();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var result = await _sut.GetOrderByIdAsync(order.Id, default);

        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.CustomerId, result.CustomerId);
        Assert.Equal(order.EventId, result.EventId);
        Assert.Equal(order.TotalAmount, result.TotalAmount);
        Assert.Equal(order.OrderStatus, result.OrderStatus);
        Assert.Empty(result.Items);
        Assert.Empty(result.SeatHolds);
    }

    [Fact]
    public async Task GetOrderByIdAsync_IncludesOrderItemsAndHolds()
    {
        var order = CreateOrder();
        _context.Orders.Add(order);
        var seatId = Guid.NewGuid();
        _context.OrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            SectionId = Guid.NewGuid(),
            SeatId = seatId,
            Price = 50m,
            CreatedAt = DateTimeOffset.UtcNow
        });
        _context.SeatHolds.Add(new SeatHold
        {
            Id = Guid.NewGuid(),
            OderId = order.Id,
            SeatId = seatId,
            SeatHoldStatus = ESeatSectionHoldStatus.Held,
            HoldExpirationTime = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _sut.GetOrderByIdAsync(order.Id, default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(50m, result.Items[0].Price);
        Assert.Single(result.SeatHolds);
        Assert.Equal(ESeatSectionHoldStatus.Held, result.SeatHolds[0].SeatHoldStatus);
    }

    [Fact]
    public async Task IsOrderExistsAsync_ReturnsFalse_WhenNotFound()
    {
        var exists = await _sut.IsOrderExistsAsync(Guid.NewGuid(), default);

        Assert.False(exists);
    }

    [Fact]
    public async Task IsOrderExistsAsync_ReturnsTrue_WhenFound()
    {
        var order = CreateOrder();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var exists = await _sut.IsOrderExistsAsync(order.Id, default);

        Assert.True(exists);
    }
}
