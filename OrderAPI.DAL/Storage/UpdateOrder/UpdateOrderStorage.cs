using Homework.Ticketing.System.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.UpdateOrder
{
    public class UpdateOrderStorage : IUpdateOrderStorage
    {
        private readonly OrderDbContext _context;

        public UpdateOrderStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task UpdateStatusAsync(Guid id, EOrderStatus status, CancellationToken ct)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

            order!.OrderStatus = status;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAmountAsync(Guid id, decimal totalAmount, ECurrency currency, CancellationToken ct)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

            order!.TotalAmount = totalAmount;
            order.Currency = currency;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
    }
}
