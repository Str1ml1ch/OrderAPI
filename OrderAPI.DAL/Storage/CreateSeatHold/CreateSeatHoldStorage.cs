using OrderAPI.Domain.Storage.CreateSeatHold;
using OrderAPI.DAL.Entities;
using OrderAPI.Domain.Enums;

namespace OrderAPI.DAL.Storage.CreateSeatHold
{
    public class CreateSeatHoldStorage : ICreateSeatHoldStorage
    {
        private readonly OrderDbContext _context;

        public CreateSeatHoldStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Guid orderId, Guid seatId, DateTimeOffset holdExpirationTime, CancellationToken ct)
        {
            var hold = new SeatHold
            {
                Id = Guid.NewGuid(),
                OderId = orderId,
                SeatId = seatId,
                SeatHoldStatus = ESeatSectionHoldStatus.Held,
                HoldExpirationTime = holdExpirationTime,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.SeatHolds.Add(hold);
            await _context.SaveChangesAsync(ct);

            return hold.Id;
        }
    }
}
