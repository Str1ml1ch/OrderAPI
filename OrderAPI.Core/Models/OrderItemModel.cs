namespace OrderAPI.Core.Models
{
    public class OrderItemModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid SectionId { get; set; }
        public Guid? SeatId { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
