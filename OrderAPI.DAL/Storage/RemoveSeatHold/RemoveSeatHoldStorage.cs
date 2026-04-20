using Microsoft.EntityFrameworkCore;

namespace OrderAPI.DAL.Storage.RemoveSeatHold
{
    public class RemoveSeatHoldStorage : IRemoveSeatHoldStorage
    {
        private readonly OrderDbContext _context;

        public RemoveSeatHoldStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task RemoveByIdAsync(Guid id, CancellationToken ct)
        {
            var hold = await _context.SeatHolds.FirstOrDefaultAsync(sh => sh.Id == id, ct);

            _context.SeatHolds.Remove(hold!);
            await _context.SaveChangesAsync(ct);
        }

        public async Task RemoveAllByOrderIdAsync(Guid orderId, CancellationToken ct)
        {
            var holds = await _context.SeatHolds
                .Where(sh => sh.OderId == orderId)
                .ToListAsync(ct);

            _context.SeatHolds.RemoveRange(holds);
            await _context.SaveChangesAsync(ct);
        }
    }
}
