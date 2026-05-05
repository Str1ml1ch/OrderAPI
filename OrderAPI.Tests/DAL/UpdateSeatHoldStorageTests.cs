using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;
using OrderAPI.DAL;
using OrderAPI.DAL.Entities;
using OrderAPI.DAL.Storage.UpdateSeatHold;
using OrderAPI.Domain.Storage.UpdateSeatHold;

namespace OrderAPI.Tests.DAL;

public class UpdateSeatHoldStorageTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly UpdateSeatHoldStorage _sut;

    public UpdateSeatHoldStorageTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new OrderDbContext(options);
        _sut = new UpdateSeatHoldStorage(_context);
    }

    public void Dispose() => _context.Dispose();

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
