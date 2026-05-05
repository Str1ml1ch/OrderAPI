namespace OrderAPI.Domain.Models
{
    public class CartItemModel
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public Guid? SeatId { get; set; }
        public decimal Price { get; set; }
    }
}
