using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Domain.Models
{
    public class CatalogSeatModel
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public string? Row { get; set; }
        public string? Number { get; set; }
        public CatalogSeatPriceOptionModel PriceOption { get; set; } = null!;
    }
}
