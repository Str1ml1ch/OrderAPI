using OrderAPI.Core.Enums;

namespace OrderAPI.Core.Models
{
    public class SeatStatusInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public static SeatStatusInfoModel From(ESeatStatus status) => new()
        {
            Id = (int)status,
            Name = status.ToString()
        };
    }
}
