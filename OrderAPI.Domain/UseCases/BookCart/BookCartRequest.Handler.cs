using MediatR;
using Microsoft.Extensions.Logging;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Messaging;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Storage.UpdateOrder;
using OrderAPI.Domain.Storage.UpdateSeatHold;
using OrderAPI.Domain.Exceptions;
using OrderAPI.Domain.Services;

namespace OrderAPI.Domain.UseCases.BookCart
{
    public class BookCartRequestHandler : IRequestHandler<BookCartRequest, BookCartResponseModel>
    {
        private readonly IGetOrderByIdStorage _orderStorage;
        private readonly IUpdateOrderStorage _updateOrderStorage;
        private readonly IUpdateSeatHoldStorage _updateHoldStorage;
        private readonly IPaymentApiClient _paymentClient;
        private readonly IEventCacheInvalidator _cacheInvalidator;
        private readonly ILogger<BookCartRequestHandler> _logger;
        private readonly INotificationPublisher _notificationPublisher;

        public BookCartRequestHandler(
            IGetOrderByIdStorage orderStorage,
            IUpdateOrderStorage updateOrderStorage,
            IUpdateSeatHoldStorage updateHoldStorage,
            IPaymentApiClient paymentClient,
            IEventCacheInvalidator cacheInvalidator,
            ILogger<BookCartRequestHandler> logger,
            INotificationPublisher notificationPublisher)
        {
            _orderStorage = orderStorage;
            _updateOrderStorage = updateOrderStorage;
            _updateHoldStorage = updateHoldStorage;
            _paymentClient = paymentClient;
            _cacheInvalidator = cacheInvalidator;
            _logger = logger;
            _notificationPublisher = notificationPublisher;
        }

        public async Task<BookCartResponseModel> Handle(BookCartRequest request, CancellationToken cancellationToken)
        {
            var order = await _orderStorage.GetOrderByIdAsync(request.CartId, cancellationToken);
            if (order is null) throw new OrderNotFoundException(request.CartId);
            if (order.OrderStatus != EOrderStatus.Pending) throw new OrderNotPendingException(request.CartId);

            await _updateHoldStorage.UpdateAllByOrderIdAsync(request.CartId, ESeatSectionHoldStatus.Confirmed, cancellationToken);
            await _updateOrderStorage.UpdateStatusAsync(request.CartId, EOrderStatus.Confirmed, cancellationToken);

            var paymentId = await _paymentClient.CreatePaymentAsync(request.CartId, order.TotalAmount, order.Currency, cancellationToken);

            await _notificationPublisher.PublishAsync(new NotificationMessage
            {
                NotificationId = Guid.NewGuid(),
                OperationName = "TicketSuccessfullyCheckedOut",
                Timestamp = DateTimeOffset.UtcNow,
                Parameters = new NotificationParameters
                {
                    CustomerEmail = request.CustomerEmail,
                    CustomerName = request.CustomerName ?? request.CustomerEmail
                },
                Content = new NotificationContent
                {
                    OrderAmount = order.TotalAmount,
                    Currency = order.Currency.ToString(),
                    EventId = order.EventId,
                    OrderSummary = $"Order {order.Id} confirmed. {order.Items.Count} ticket(s) for event {order.EventId}. Total: {order.TotalAmount} {order.Currency}"
                }
            }, cancellationToken);

            try
            {
                await _cacheInvalidator.InvalidateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed for cart {CartId}. Stale cache will expire naturally.", request.CartId);
            }

            return new BookCartResponseModel { PaymentId = paymentId };
        }
    }
}
