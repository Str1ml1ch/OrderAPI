using OrderAPI.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.DAL.Storage.UpdateSectionHold
{
    public class UpdateSectionHoldStorage : IUpdateSectionHoldStorage
    {
        private readonly OrderDbContext _context;

        public UpdateSectionHoldStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task UpdateStatusAsync(Guid id, ESeatSectionHoldStatus status, CancellationToken ct)
        {
            var hold = await _context.SectionHolds.FirstOrDefaultAsync(sh => sh.Id == id, ct);

            hold!.SectionHoldStatus = status;
            hold.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
    }
}
