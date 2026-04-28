using OrderAPI.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.DAL.Storage.UpdateSeatHold
{
    public class UpdateSeatHoldStorage : IUpdateSeatHoldStorage
    {
        private readonly OrderDbContext _context;

        public UpdateSeatHoldStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task UpdateStatusAsync(Guid id, ESeatSectionHoldStatus status, CancellationToken ct)
        {
            var hold = await _context.SeatHolds.FirstOrDefaultAsync(sh => sh.Id == id, ct);

            hold!.SeatHoldStatus = status;
            hold.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAllByOrderIdAsync(Guid orderId, ESeatSectionHoldStatus status, CancellationToken ct)
        {
            var holds = await _context.SeatHolds
                .Where(sh => sh.OderId == orderId)
                .ToListAsync(ct);

            foreach (var hold in holds)
            {
                hold.SeatHoldStatus = status;
                hold.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
