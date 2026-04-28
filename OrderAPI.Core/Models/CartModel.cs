using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Core.Enums;

namespace OrderAPI.Core.Models
{
    public class CartModel
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public decimal TotalAmount { get; set; }
        public ECurrency Currency { get; set; }
        public EOrderStatus Status { get; set; }
        public List<CartItemModel> Items { get; set; } = [];
    }
}
