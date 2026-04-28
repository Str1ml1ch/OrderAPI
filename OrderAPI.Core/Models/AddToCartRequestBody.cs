using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Core.Models
{
    public class AddToCartRequestBody
    {
        public Guid EventId { get; set; }
        public Guid SectionId { get; set; }
        public Guid SeatId { get; set; }
        public decimal Price { get; set; }
        public ECurrency Currency { get; set; }
    }
}
