using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderAPI.Domain.Enums;
using OrderAPI.DAL;
using OrderAPI.DAL.Entities;
using OrderAPI.DAL.Storage.GetSeatStatuses;
using OrderAPI.Tests.DAL.Infrastructure;

namespace OrderAPI.Tests.DAL;

[Collection("SqlServer")]
public class GetSeatStatusesStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private OrderDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private GetSeatStatusesStorage _sut = null!;

    public GetSeatStatusesStorageTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = new OrderDbContext(
            new DbContextOptionsBuilder<OrderDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options);
        _transaction = await _context.Database.BeginTransactionAsync();
        _sut = new GetSeatStatusesStorage(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    private Order CreateOrder(EOrderStatus status = EOrderStatus.Pending)
        => new()
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = Homework.Ticketing.System.Shared.Enums.ECurrency.USD,
            OrderStatus = status,
            CreatedAt = DateTimeOffset.UtcNow
        };

    [Fact]
    public async Task GetAsync_ReturnsAvailable_WhenSeatHasNoHoldsOrOrders()
    {
        var seatId = Guid.NewGuid();

        var result = await _sut.GetAsync([seatId], default);

        Assert.Equal(ESeatStatus.Available, result[seatId]);
    }

    [Fact]
    public async Task GetAsync_ReturnsReserved_WhenSeatHasActiveHeld()
    {
        var seatId = Guid.NewGuid();
        var order = CreateOrder();
        _context.Orders.Add(order);
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

        var result = await _sut.GetAsync([seatId], default);

        Assert.Equal(ESeatStatus.Reserved, result[seatId]);
    }

    [Fact]
    public async Task GetAsync_ReturnsAvailable_WhenHoldIsExpired()
    {
        var seatId = Guid.NewGuid();
        var order = CreateOrder();
        _context.Orders.Add(order);
        _context.SeatHolds.Add(new SeatHold
        {
            Id = Guid.NewGuid(),
            OderId = order.Id,
            SeatId = seatId,
            SeatHoldStatus = ESeatSectionHoldStatus.Held,
            HoldExpirationTime = DateTimeOffset.UtcNow.AddMinutes(-5),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _sut.GetAsync([seatId], default);

        Assert.Equal(ESeatStatus.Available, result[seatId]);
    }

    [Fact]
    public async Task GetAsync_ReturnsSold_WhenSeatIsInConfirmedOrder()
    {
        var seatId = Guid.NewGuid();
        var order = CreateOrder(EOrderStatus.Confirmed);
        _context.Orders.Add(order);
        _context.OrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            SectionId = Guid.NewGuid(),
            SeatId = seatId,
            Price = 100m,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _sut.GetAsync([seatId], default);

        Assert.Equal(ESeatStatus.Sold, result[seatId]);
    }

    [Fact]
    public async Task GetAsync_ReturnsMixedStatuses_ForMultipleSeats()
    {
        var availableId = Guid.NewGuid();
        var reservedId = Guid.NewGuid();
        var soldId = Guid.NewGuid();

        var pendingOrder = CreateOrder(EOrderStatus.Pending);
        var confirmedOrder = CreateOrder(EOrderStatus.Confirmed);
        _context.Orders.AddRange(pendingOrder, confirmedOrder);

        _context.SeatHolds.Add(new SeatHold
        {
            Id = Guid.NewGuid(),
            OderId = pendingOrder.Id,
            SeatId = reservedId,
            SeatHoldStatus = ESeatSectionHoldStatus.Held,
            HoldExpirationTime = DateTimeOffset.UtcNow.AddHours(1),
            CreatedAt = DateTimeOffset.UtcNow
        });
        _context.OrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = confirmedOrder.Id,
            SectionId = Guid.NewGuid(),
            SeatId = soldId,
            Price = 80m,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _sut.GetAsync([availableId, reservedId, soldId], default);

        Assert.Equal(ESeatStatus.Available, result[availableId]);
        Assert.Equal(ESeatStatus.Reserved, result[reservedId]);
        Assert.Equal(ESeatStatus.Sold, result[soldId]);
    }
}
