using MediatR;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Storage.UpdateOrder;
using OrderAPI.Domain.Storage.UpdateSeatHold;
using OrderAPI.Domain.Exceptions;

namespace OrderAPI.Domain.UseCases.CancelOrder
{
    public class CancelOrderRequestHandler : IRequestHandler<CancelOrderRequest, bool>
    {
        private readonly IGetOrderByIdStorage _orderStorage;
        private readonly IUpdateOrderStorage _updateOrderStorage;
        private readonly IUpdateSeatHoldStorage _updateHoldStorage;

        public CancelOrderRequestHandler(
            IGetOrderByIdStorage orderStorage,
            IUpdateOrderStorage updateOrderStorage,
            IUpdateSeatHoldStorage updateHoldStorage)
        {
            _orderStorage = orderStorage;
            _updateOrderStorage = updateOrderStorage;
            _updateHoldStorage = updateHoldStorage;
        }

        public async Task<bool> Handle(CancelOrderRequest request, CancellationToken cancellationToken)
        {
            var exists = await _orderStorage.IsOrderExistsAsync(request.OrderId, cancellationToken);
            if (!exists) throw new OrderNotFoundException(request.OrderId);

            await _updateHoldStorage.UpdateAllByOrderIdAsync(request.OrderId, ESeatSectionHoldStatus.Released, cancellationToken);
            await _updateOrderStorage.UpdateStatusAsync(request.OrderId, EOrderStatus.Cancelled, cancellationToken);
            return true;
        }
    }
}
