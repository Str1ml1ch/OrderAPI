using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;
using OrderAPI.DAL;
using OrderAPI.DAL.Storage.CreateOrder;
using OrderAPI.Domain.Storage.CreateOrder;

namespace OrderAPI.Tests.DAL;

public class CreateOrderStorageTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly CreateOrderStorage _sut;

    public CreateOrderStorageTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new OrderDbContext(options);
        _sut = new CreateOrderStorage(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_PersistsOrder_AndReturnsNewId()
    {
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var id = await _sut.CreateAsync(
            customerId, eventId, 150m,
            Homework.Ticketing.System.Shared.Enums.ECurrency.EUR,
            EOrderStatus.Pending, default);

        var saved = await _context.Orders.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(customerId, saved.CustomerId);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal(150m, saved.TotalAmount);
        Assert.Equal(EOrderStatus.Pending, saved.OrderStatus);
    }

    [Fact]
    public async Task CreateWithIdAsync_PersistsOrder_WithSpecifiedId()
    {
        var specifiedId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var returnedId = await _sut.CreateWithIdAsync(
            specifiedId, customerId, eventId, 200m,
            Homework.Ticketing.System.Shared.Enums.ECurrency.USD,
            EOrderStatus.Pending, default);

        Assert.Equal(specifiedId, returnedId);
        var saved = await _context.Orders.FindAsync(specifiedId);
        Assert.NotNull(saved);
        Assert.Equal(customerId, saved.CustomerId);
        Assert.Equal(200m, saved.TotalAmount);
    }
}
