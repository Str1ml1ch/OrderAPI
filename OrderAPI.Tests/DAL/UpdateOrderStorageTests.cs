using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;
using OrderAPI.DAL;
using OrderAPI.DAL.Entities;
using OrderAPI.DAL.Storage.UpdateOrder;
using OrderAPI.Domain.Storage.UpdateOrder;

namespace OrderAPI.Tests.DAL;

public class UpdateOrderStorageTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly UpdateOrderStorage _sut;

    public UpdateOrderStorageTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new OrderDbContext(options);
        _sut = new UpdateOrderStorage(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<Order> AddOrder(EOrderStatus status = EOrderStatus.Pending)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = Homework.Ticketing.System.Shared.Enums.ECurrency.USD,
            OrderStatus = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesOrderStatus()
    {
        var order = await AddOrder(EOrderStatus.Pending);

        await _sut.UpdateStatusAsync(order.Id, EOrderStatus.Confirmed, default);

        var updated = await _context.Orders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        Assert.Equal(EOrderStatus.Confirmed, updated.OrderStatus);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAmountAsync_ChangesTotalAmountAndCurrency()
    {
        var order = await AddOrder();

        await _sut.UpdateAmountAsync(order.Id, 250m,
            Homework.Ticketing.System.Shared.Enums.ECurrency.EUR, default);

        var updated = await _context.Orders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        Assert.Equal(250m, updated.TotalAmount);
        Assert.Equal(Homework.Ticketing.System.Shared.Enums.ECurrency.EUR, updated.Currency);
        Assert.NotNull(updated.UpdatedAt);
    }
}
