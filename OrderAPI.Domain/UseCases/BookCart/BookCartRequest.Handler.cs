using MediatR;
using Microsoft.Extensions.Logging;
using OrderAPI.Domain.Enums;
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

        public BookCartRequestHandler(
            IGetOrderByIdStorage orderStorage,
            IUpdateOrderStorage updateOrderStorage,
            IUpdateSeatHoldStorage updateHoldStorage,
            IPaymentApiClient paymentClient,
            IEventCacheInvalidator cacheInvalidator,
            ILogger<BookCartRequestHandler> logger)
        {
            _orderStorage = orderStorage;
            _updateOrderStorage = updateOrderStorage;
            _updateHoldStorage = updateHoldStorage;
            _paymentClient = paymentClient;
            _cacheInvalidator = cacheInvalidator;
            _logger = logger;
        }

        public async Task<BookCartResponseModel> Handle(BookCartRequest request, CancellationToken cancellationToken)
        {
            var order = await _orderStorage.GetOrderByIdAsync(request.CartId, cancellationToken);
            if (order is null) throw new OrderNotFoundException(request.CartId);
            if (order.OrderStatus != EOrderStatus.Pending) throw new OrderNotPendingException(request.CartId);

            await _updateHoldStorage.UpdateAllByOrderIdAsync(request.CartId, ESeatSectionHoldStatus.Confirmed, cancellationToken);
            await _updateOrderStorage.UpdateStatusAsync(request.CartId, EOrderStatus.Confirmed, cancellationToken);

            var paymentId = await _paymentClient.CreatePaymentAsync(request.CartId, order.TotalAmount, order.Currency, cancellationToken);

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
