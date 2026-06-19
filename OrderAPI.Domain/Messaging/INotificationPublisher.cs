namespace OrderAPI.Domain.Messaging
{
    public interface INotificationPublisher
    {
        Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
