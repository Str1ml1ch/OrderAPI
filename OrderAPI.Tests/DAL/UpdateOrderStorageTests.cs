using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderAPI.Domain.Enums;
using OrderAPI.DAL;
using OrderAPI.DAL.Entities;
using OrderAPI.DAL.Storage.UpdateOrder;
using OrderAPI.Tests.DAL.Infrastructure;

namespace OrderAPI.Tests.DAL;

[Collection("SqlServer")]
public class UpdateOrderStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private OrderDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private UpdateOrderStorage _sut = null!;

    public UpdateOrderStorageTests(SqlServerContainerFixture fixture)
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
        _sut = new UpdateOrderStorage(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

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
