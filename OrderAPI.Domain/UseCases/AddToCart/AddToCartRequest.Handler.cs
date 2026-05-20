using MediatR;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.Services;
using OrderAPI.Domain.Storage.AddSeatToCart;
using OrderAPI.Domain.Storage.CreateOrder;
using OrderAPI.Domain.Storage.CreateOrderItem;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Storage.UpdateOrder;

namespace OrderAPI.Domain.UseCases.AddToCart
{
    public class AddToCartRequestHandler : IRequestHandler<AddToCartRequest, CartModel>
    {
        private readonly IGetOrderByIdStorage _orderStorage;
        private readonly ICreateOrderStorage _createOrderStorage;
        private readonly ICreateOrderItemStorage _createItemStorage;
        private readonly IAddSeatToCartStorage _addSeatToCartStorage;
        private readonly IUpdateOrderStorage _updateOrderStorage;
        private readonly IEventCacheInvalidator _cacheInvalidator;

        public AddToCartRequestHandler(
            IGetOrderByIdStorage orderStorage,
            ICreateOrderStorage createOrderStorage,
            ICreateOrderItemStorage createItemStorage,
            IAddSeatToCartStorage addSeatToCartStorage,
            IUpdateOrderStorage updateOrderStorage,
            IEventCacheInvalidator cacheInvalidator)
        {
            _orderStorage = orderStorage;
            _createOrderStorage = createOrderStorage;
            _createItemStorage = createItemStorage;
            _addSeatToCartStorage = addSeatToCartStorage;
            _updateOrderStorage = updateOrderStorage;
            _cacheInvalidator = cacheInvalidator;
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

            await _addSeatToCartStorage.CreateSeatHoldAsync(
                request.CartId, body.SeatId, DateTimeOffset.UtcNow.AddMinutes(30), cancellationToken);

            var newTotal = order!.Items.Sum(i => i.Price) + body.Price;
            await _updateOrderStorage.UpdateAmountAsync(request.CartId, newTotal, body.Currency, cancellationToken);

            await _cacheInvalidator.InvalidateAsync(cancellationToken);

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
