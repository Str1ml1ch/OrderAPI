using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Core.Models
{
    public class CatalogSeatPriceOptionModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public ECurrency Currency { get; set; }
    }
}
