using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderAPI.Domain.Enums;
using OrderAPI.DAL;
using OrderAPI.DAL.Storage.CreateOrder;
using OrderAPI.Tests.DAL.Infrastructure;

namespace OrderAPI.Tests.DAL;

[Collection("SqlServer")]
public class CreateOrderStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private OrderDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private CreateOrderStorage _sut = null!;

    public CreateOrderStorageTests(SqlServerContainerFixture fixture)
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
        _sut = new CreateOrderStorage(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

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
