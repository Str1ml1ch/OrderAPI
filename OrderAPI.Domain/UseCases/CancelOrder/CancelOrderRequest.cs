using MediatR;

namespace OrderAPI.Domain.UseCases.CancelOrder
{
    public class CancelOrderRequest : IRequest<bool>
    {
        public Guid OrderId { get; set; }
    }
}
