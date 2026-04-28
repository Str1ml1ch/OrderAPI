using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Domain.UseCases.CancelOrder;

namespace OrderAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPut("{order_id}/cancel")]
        public async Task<IActionResult> Cancel(Guid order_id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CancelOrderRequest { OrderId = order_id }, cancellationToken);
            return NoContent();
        }
    }
}
