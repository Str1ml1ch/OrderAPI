namespace OrderAPI.Domain.Messaging
{
    public class NotificationMessage
    {
        public Guid NotificationId { get; set; }
        public string OperationName { get; set; } = null!;
        public DateTimeOffset Timestamp { get; set; }
        public NotificationParameters Parameters { get; set; } = null!;
        public NotificationContent Content { get; set; } = null!;
    }

    public class NotificationParameters
    {
        public string CustomerEmail { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
    }

    public class NotificationContent
    {
        public decimal? OrderAmount { get; set; }
        public string? Currency { get; set; }
        public string? OrderSummary { get; set; }
        public Guid? EventId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? SeatId { get; set; }
    }
}
