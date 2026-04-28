using MediatR;

namespace OrderAPI.Domain.UseCases.RemoveFromCart
{
    public class RemoveFromCartRequest : IRequest<bool>
    {
        public Guid CartId { get; set; }
        public Guid SeatId { get; set; }
    }
}
