using Shared.DAL.Entities;

namespace OrderAPI.DAL.Entities
{
    public class OrderItem : BaseDbEntity
    {
        public Guid OrderId { get; set; }
        public Guid SectionId { get; set; }
        public Guid? SeatId { get; set; }
        public decimal Price { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}
