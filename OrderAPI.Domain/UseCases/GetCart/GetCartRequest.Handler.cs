using MediatR;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Exceptions;

namespace OrderAPI.Domain.UseCases.GetCart
{
    public class GetCartRequestHandler : IRequestHandler<GetCartRequest, CartModel>
    {
        private readonly IGetOrderByIdStorage _orderStorage;

        public GetCartRequestHandler(IGetOrderByIdStorage orderStorage)
        {
            _orderStorage = orderStorage;
        }

        public async Task<CartModel> Handle(GetCartRequest request, CancellationToken cancellationToken)
        {
            var order = await _orderStorage.GetOrderByIdAsync(request.CartId, cancellationToken);
            if (order is null) throw new OrderNotFoundException(request.CartId);

            return new CartModel
            {
                Id = order.Id,
                EventId = order.EventId,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.OrderStatus,
                Items = order.Items.Select(i => new CartItemModel
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
