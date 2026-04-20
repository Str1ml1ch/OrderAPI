using OrderAPI.Core.Enums;
using Shared.DAL.Entities;

namespace OrderAPI.DAL.Entities
{
    public class SeatHold : BaseDbEntity
    {
        public Guid? SeatId { get; set; }
        public Guid? OderId { get; set; }
        public ESeatSectionHoldStatus SeatHoldStatus { get; set; }
        public DateTimeOffset HoldExpirationTime { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}
