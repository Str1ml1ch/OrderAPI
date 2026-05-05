using OrderAPI.Domain.Storage.RemoveSectionHold;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.DAL.Storage.RemoveSectionHold
{
    public class RemoveSectionHoldStorage : IRemoveSectionHoldStorage
    {
        private readonly OrderDbContext _context;

        public RemoveSectionHoldStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task RemoveByIdAsync(Guid id, CancellationToken ct)
        {
            var hold = await _context.SectionHolds.FirstOrDefaultAsync(sh => sh.Id == id, ct);

            _context.SectionHolds.Remove(hold!);
            await _context.SaveChangesAsync(ct);
        }

        public async Task RemoveAllByOrderIdAsync(Guid orderId, CancellationToken ct)
        {
            var holds = await _context.SectionHolds
                .Where(sh => sh.OderId == orderId)
                .ToListAsync(ct);

            _context.SectionHolds.RemoveRange(holds);
            await _context.SaveChangesAsync(ct);
        }
    }
}
