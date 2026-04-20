using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Core.Enums;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Storage.CreateOrder
{
    public class CreateOrderStorage : ICreateOrderStorage
    {
        private readonly OrderDbContext _context;

        public CreateOrderStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Guid customerId, Guid eventId, decimal totalAmount, ECurrency currency, EOrderStatus status, CancellationToken ct)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                EventId = eventId,
                TotalAmount = totalAmount,
                Currency = currency,
                OrderStatus = status,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            return order.Id;
        }
    }
}
