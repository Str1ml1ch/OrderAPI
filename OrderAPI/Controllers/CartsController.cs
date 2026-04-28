using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Core.Models;
using OrderAPI.Domain.UseCases.AddToCart;
using OrderAPI.Domain.UseCases.BookCart;
using OrderAPI.Domain.UseCases.GetCart;
using OrderAPI.Domain.UseCases.RemoveFromCart;
using System.Security.Claims;

namespace OrderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders/carts")]
    public class CartsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{cart_id}")]
        public async Task<IActionResult> GetCart(Guid cart_id, CancellationToken cancellationToken)
        {
            var cart = await _mediator.Send(new GetCartRequest { CartId = cart_id }, cancellationToken);
            return Ok(cart);
        }

        [HttpPost("{cart_id}")]
        public async Task<IActionResult> AddToCart(
            Guid cart_id,
            [FromBody] AddToCartRequestBody body,
            CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var customerId))
                return Unauthorized();

            var cart = await _mediator.Send(new AddToCartRequest
            {
                CartId = cart_id,
                CustomerId = customerId,
                Body = body
            }, cancellationToken);
            return Ok(cart);
        }

        [HttpDelete("{cart_id}/events/{event_id}/seats/{seat_id}")]
        public async Task<IActionResult> RemoveFromCart(
            Guid cart_id,
            Guid event_id,
            Guid seat_id,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(new RemoveFromCartRequest
            {
                CartId = cart_id,
                SeatId = seat_id
            }, cancellationToken);
            return NoContent();
        }

        [HttpPut("{cart_id}/book")]
        public async Task<IActionResult> BookCart(Guid cart_id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BookCartRequest { CartId = cart_id }, cancellationToken);
            return Ok(result);
        }
    }
}
