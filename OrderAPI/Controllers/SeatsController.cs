using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Domain.UseCases.GetSeatStatuses;

namespace OrderAPI.Controllers
{
    [ApiController]
    [Route("api/orders/seats")]
    public class SeatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SeatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses(
            [FromQuery] Guid[] seatIds,
            CancellationToken cancellationToken)
        {
            var statuses = await _mediator.Send(new GetSeatStatusesRequest { SeatIds = seatIds }, cancellationToken);
            return Ok(statuses);
        }
    }
}
