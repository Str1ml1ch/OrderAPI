using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Domain.Enums;
using Shared.DAL.Entities;

namespace OrderAPI.DAL.Entities
{
    public class Order : BaseDbEntity
    {
        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }
        public decimal TotalAmount { get; set; }
        public ECurrency Currency { get; set; }
        public EOrderStatus OrderStatus { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();
            public virtual ICollection<SectionHold> SectionHolds { get; set; } = new List<SectionHold>();
    }
}
