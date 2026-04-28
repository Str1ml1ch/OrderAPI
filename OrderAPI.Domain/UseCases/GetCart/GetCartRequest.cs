using MediatR;
using OrderAPI.Core.Models;

namespace OrderAPI.Domain.UseCases.GetCart
{
    public class GetCartRequest : IRequest<CartModel>
    {
        public Guid CartId { get; set; }
    }
}
