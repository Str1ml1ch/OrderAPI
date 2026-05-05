using OrderAPI.Domain.Storage.CreateSectionHold;
using OrderAPI.DAL.Entities;
using OrderAPI.Domain.Enums;

namespace OrderAPI.DAL.Storage.CreateSectionHold
{
    public class CreateSectionHoldStorage : ICreateSectionHoldStorage
    {
        private readonly OrderDbContext _context;

        public CreateSectionHoldStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Guid orderId, Guid sectionId, int quantity, DateTimeOffset holdExpirationTime, CancellationToken ct)
        {
            var hold = new SectionHold
            {
                Id = Guid.NewGuid(),
                OderId = orderId,
                SectionId = sectionId,
                Quantity = quantity,
                SectionHoldStatus = ESeatSectionHoldStatus.Held,
                HoldExpirationTime = holdExpirationTime,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.SectionHolds.Add(hold);
            await _context.SaveChangesAsync(ct);

            return hold.Id;
        }
    }
}
