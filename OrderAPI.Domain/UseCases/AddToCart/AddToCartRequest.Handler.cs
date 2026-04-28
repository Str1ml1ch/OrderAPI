using MediatR;
using OrderAPI.Core.Enums;
using OrderAPI.Core.Models;
using OrderAPI.DAL.Storage.CreateOrder;
using OrderAPI.DAL.Storage.CreateOrderItem;
using OrderAPI.DAL.Storage.CreateSeatHold;
using OrderAPI.DAL.Storage.GetOrderById;
using OrderAPI.DAL.Storage.UpdateOrder;

namespace OrderAPI.Domain.UseCases.AddToCart
{
    public class AddToCartRequestHandler : IRequestHandler<AddToCartRequest, CartModel>
    {
        private readonly IGetOrderByIdStorage _orderStorage;
        private readonly ICreateOrderStorage _createOrderStorage;
        private readonly ICreateOrderItemStorage _createItemStorage;
        private readonly ICreateSeatHoldStorage _createHoldStorage;
        private readonly IUpdateOrderStorage _updateOrderStorage;

        public AddToCartRequestHandler(
            IGetOrderByIdStorage orderStorage,
            ICreateOrderStorage createOrderStorage,
            ICreateOrderItemStorage createItemStorage,
            ICreateSeatHoldStorage createHoldStorage,
            IUpdateOrderStorage updateOrderStorage)
        {
            _orderStorage = orderStorage;
            _createOrderStorage = createOrderStorage;
            _createItemStorage = createItemStorage;
            _createHoldStorage = createHoldStorage;
            _updateOrderStorage = updateOrderStorage;
        }

        public async Task<CartModel> Handle(AddToCartRequest request, CancellationToken cancellationToken)
        {
            var body = request.Body;
            var order = await _orderStorage.GetOrderByIdAsync(request.CartId, cancellationToken);
            if (order is null)
            {
                await _createOrderStorage.CreateWithIdAsync(
                    request.CartId, request.CustomerId, body.EventId,
                    0, body.Currency, EOrderStatus.Pending, cancellationToken);
                order = await _orderStorage.GetOrderByIdAsync(request.CartId, cancellationToken);
            }

            await _createItemStorage.CreateAsync(request.CartId, body.SectionId, body.SeatId, body.Price, cancellationToken);
            await _createHoldStorage.CreateAsync(request.CartId, body.SeatId, DateTimeOffset.UtcNow.AddMinutes(30), cancellationToken);

            var newTotal = order!.Items.Sum(i => i.Price) + body.Price;
            await _updateOrderStorage.UpdateAmountAsync(request.CartId, newTotal, body.Currency, cancellationToken);

            var updated = await _orderStorage.GetOrderByIdAsync(request.CartId, cancellationToken);
            return new CartModel
            {
                Id = updated!.Id,
                EventId = updated.EventId,
                TotalAmount = updated.TotalAmount,
                Currency = updated.Currency,
                Status = updated.OrderStatus,
                Items = updated.Items.Select(i => new CartItemModel
                {
                    Id = i.Id,
                    SectionId = i.SectionId,
                    SeatId = i.SeatId,
                    Price = i.Price
                }).ToList()
            };
        }
    }
}
