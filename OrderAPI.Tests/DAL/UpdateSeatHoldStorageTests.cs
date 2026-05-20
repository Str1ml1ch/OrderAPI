using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderAPI.Domain.Enums;
using OrderAPI.DAL;
using OrderAPI.DAL.Entities;
using OrderAPI.DAL.Storage.UpdateSeatHold;
using OrderAPI.Tests.DAL.Infrastructure;

namespace OrderAPI.Tests.DAL;

[Collection("SqlServer")]
public class UpdateSeatHoldStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private OrderDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private UpdateSeatHoldStorage _sut = null!;

    public UpdateSeatHoldStorageTests(SqlServerContainerFixture fixture)
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
        _sut = new UpdateSeatHoldStorage(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    private async Task<(Order order, SeatHold hold)> AddOrderWithHold(ESeatSectionHoldStatus status = ESeatSectionHoldStatus.Held)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            TotalAmount = 0,
            Currency = Homework.Ticketing.System.Shared.Enums.ECurrency.USD,
            OrderStatus = EOrderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var hold = new SeatHold
        {
            Id = Guid.NewGuid(),
            OderId = order.Id,
            SeatId = Guid.NewGuid(),
            SeatHoldStatus = status,
            HoldExpirationTime = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Orders.Add(order);
        _context.SeatHolds.Add(hold);
        await _context.SaveChangesAsync();
        return (order, hold);
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesHoldStatus()
    {
        var (_, hold) = await AddOrderWithHold();

        await _sut.UpdateStatusAsync(hold.Id, ESeatSectionHoldStatus.Confirmed, default);

        var updated = await _context.SeatHolds.AsNoTracking().FirstAsync(h => h.Id == hold.Id);
        Assert.Equal(ESeatSectionHoldStatus.Confirmed, updated.SeatHoldStatus);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAllByOrderIdAsync_UpdatesAllHoldsForOrder()
    {
        var (order, _) = await AddOrderWithHold();
        _context.SeatHolds.Add(new SeatHold
        {
            Id = Guid.NewGuid(),
            OderId = order.Id,
            SeatId = Guid.NewGuid(),
            SeatHoldStatus = ESeatSectionHoldStatus.Held,
            HoldExpirationTime = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _context.SaveChangesAsync();

        await _sut.UpdateAllByOrderIdAsync(order.Id, ESeatSectionHoldStatus.Released, default);

        var allHolds = await _context.SeatHolds.AsNoTracking()
            .Where(h => h.OderId == order.Id).ToListAsync();
        Assert.All(allHolds, h => Assert.Equal(ESeatSectionHoldStatus.Released, h.SeatHoldStatus));
    }
}
