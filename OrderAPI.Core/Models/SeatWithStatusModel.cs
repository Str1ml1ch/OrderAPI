namespace OrderAPI.Core.Models
{
    public class SeatWithStatusModel
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public string? Row { get; set; }
        public string? Number { get; set; }
        public SeatStatusInfoModel Status { get; set; } = null!;
        public CatalogSeatPriceOptionModel PriceOption { get; set; } = null!;
    }
}
