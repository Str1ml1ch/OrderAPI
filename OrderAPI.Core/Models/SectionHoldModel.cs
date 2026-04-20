using OrderAPI.Core.Enums;

namespace OrderAPI.Core.Models
{
    public class SectionHoldModel
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public Guid? OderId { get; set; }
        public ESeatSectionHoldStatus SectionHoldStatus { get; set; }
        public DateTimeOffset HoldExpirationTime { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
