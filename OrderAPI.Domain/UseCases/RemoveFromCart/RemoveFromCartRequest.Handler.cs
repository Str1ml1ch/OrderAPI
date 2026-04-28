using MediatR;
using OrderAPI.DAL.Storage.GetOrderById;
using OrderAPI.DAL.Storage.RemoveOrderItem;
using OrderAPI.DAL.Storage.RemoveSeatHold;
using OrderAPI.DAL.Storage.UpdateOrder;
using OrderAPI.Domain.Exceptions;

namespace OrderAPI.Domain.UseCases.RemoveFromCart
{
    public class RemoveFromCartRequestHandler : IRequestHandler<RemoveFromCartRequest, bool>
    {
        private readonly IGetOrderByIdStorage _orderStorage;
        private readonly IRemoveOrderItemStorage _removeItemStorage;
        private readonly IRemoveSeatHoldStorage _removeHoldStorage;
        private readonly IUpdateOrderStorage _updateOrderStorage;

        public RemoveFromCartRequestHandler(
            IGetOrderByIdStorage orderStorage,
            IRemoveOrderItemStorage removeItemStorage,
            IRemoveSeatHoldStorage removeHoldStorage,
            IUpdateOrderStorage updateOrderStorage)
        {
            _orderStorage = orderStorage;
            _removeItemStorage = removeItemStorage;
            _removeHoldStorage = removeHoldStorage;
            _updateOrderStorage = updateOrderStorage;
        }

        public async Task<bool> Handle(RemoveFromCartRequest request, CancellationToken cancellationToken)
        {
            var order = await _orderStorage.GetOrderByIdAsync(request.CartId, cancellationToken);
            if (order is null) throw new OrderNotFoundException(request.CartId);

            var item = order.Items.FirstOrDefault(i => i.SeatId == request.SeatId);
            if (item is null) throw new CartItemNotFoundException(request.SeatId);

            var hold = order.SeatHolds.FirstOrDefault(h => h.SeatId == request.SeatId);

            await _removeItemStorage.RemoveByIdAsync(item.Id, cancellationToken);
            if (hold is not null)
                await _removeHoldStorage.RemoveByIdAsync(hold.Id, cancellationToken);

            var newTotal = order.Items.Where(i => i.Id != item.Id).Sum(i => i.Price);
            await _updateOrderStorage.UpdateAmountAsync(request.CartId, newTotal, order.Currency, cancellationToken);

            return true;
        }
    }
}
