using OrderAPI.Domain.Storage.RemoveOrder;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.DAL.Storage.RemoveOrder
{
    public class RemoveOrderStorage : IRemoveOrderStorage
    {
        private readonly OrderDbContext _context;

        public RemoveOrderStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task RemoveByIdAsync(Guid id, CancellationToken ct)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

            _context.Orders.Remove(order!);
            await _context.SaveChangesAsync(ct);
        }
    }
}
