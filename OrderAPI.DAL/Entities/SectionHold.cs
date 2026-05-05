using OrderAPI.Domain.Enums;
using Shared.DAL.Entities;

namespace OrderAPI.DAL.Entities
{
    public class SectionHold : BaseDbEntity
    {
        public Guid SectionId { get; set; }
        public Guid? OderId { get; set; }
        public ESeatSectionHoldStatus SectionHoldStatus { get; set; }
        public DateTimeOffset HoldExpirationTime { get; set; }
        public int Quantity { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}
