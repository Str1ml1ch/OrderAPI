using MediatR;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.UseCases.AddToCart
{
    public class AddToCartRequest : IRequest<CartModel>
    {
        public Guid CartId { get; set; }
        public Guid CustomerId { get; set; }
        public required AddToCartRequestBody Body { get; set; }
    }
}
