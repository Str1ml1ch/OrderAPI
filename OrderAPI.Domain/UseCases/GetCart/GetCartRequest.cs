using MediatR;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.UseCases.GetCart
{
    public class GetCartRequest : IRequest<CartModel>
    {
        public Guid CartId { get; set; }
    }
}
