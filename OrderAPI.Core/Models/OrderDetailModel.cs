using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Core.Enums;

namespace OrderAPI.Core.Models
{
    public class OrderDetailModel
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }
        public decimal TotalAmount { get; set; }
        public ECurrency Currency { get; set; }
        public EOrderStatus OrderStatus { get; set; }
        public List<OrderItemModel> Items { get; set; } = [];
        public List<SeatHoldModel> SeatHolds { get; set; } = [];
        public List<SectionHoldModel> SectionHolds { get; set; } = [];
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
