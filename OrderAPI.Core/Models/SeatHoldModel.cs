using OrderAPI.Core.Enums;

namespace OrderAPI.Core.Models
{
    public class SeatHoldModel
    {
        public Guid Id { get; set; }
        public Guid? SeatId { get; set; }
        public Guid? OderId { get; set; }
        public ESeatSectionHoldStatus SeatHoldStatus { get; set; }
        public DateTimeOffset HoldExpirationTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
