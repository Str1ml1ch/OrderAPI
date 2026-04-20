using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Storage.CreateOrderItem
{
    public class CreateOrderItemStorage : ICreateOrderItemStorage
    {
        private readonly OrderDbContext _context;

        public CreateOrderItemStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Guid orderId, Guid sectionId, Guid? seatId, decimal price, CancellationToken ct)
        {
            var item = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                SectionId = sectionId,
                SeatId = seatId,
                Price = price,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.OrderItems.Add(item);
            await _context.SaveChangesAsync(ct);

            return item.Id;
        }
    }
}
