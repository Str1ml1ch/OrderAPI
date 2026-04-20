using Microsoft.EntityFrameworkCore;

namespace OrderAPI.DAL.Storage.RemoveOrderItem
{
    public class RemoveOrderItemStorage : IRemoveOrderItemStorage
    {
        private readonly OrderDbContext _context;

        public RemoveOrderItemStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task RemoveByIdAsync(Guid id, CancellationToken ct)
        {
            var item = await _context.OrderItems.FirstOrDefaultAsync(oi => oi.Id == id, ct);

            _context.OrderItems.Remove(item!);
            await _context.SaveChangesAsync(ct);
        }

        public async Task RemoveAllByOrderIdAsync(Guid orderId, CancellationToken ct)
        {
            var items = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync(ct);

            _context.OrderItems.RemoveRange(items);
            await _context.SaveChangesAsync(ct);
        }
    }
}
