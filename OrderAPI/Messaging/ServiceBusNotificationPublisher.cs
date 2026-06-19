using Azure.Messaging.ServiceBus;
using OrderAPI.Domain.Messaging;
using System.Text.Json;

namespace OrderAPI.Messaging
{
    public class ServiceBusNotificationPublisher : INotificationPublisher, IAsyncDisposable
    {
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusClient _client;
        private readonly ILogger<ServiceBusNotificationPublisher> _logger;

        public ServiceBusNotificationPublisher(IConfiguration configuration, ILogger<ServiceBusNotificationPublisher> logger)
        {
            _logger = logger;
            var connectionString = configuration["ServiceBus:ConnectionString"]
                ?? throw new InvalidOperationException("ServiceBus:ConnectionString is not configured.");
            var queueName = configuration["ServiceBus:QueueName"] ?? "notifications";

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);
        }

        public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                MessageId = message.NotificationId.ToString(),
                Subject = message.OperationName,
                ContentType = "application/json"
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation("Published notification {NotificationId} for operation {Operation}", message.NotificationId, message.OperationName);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
