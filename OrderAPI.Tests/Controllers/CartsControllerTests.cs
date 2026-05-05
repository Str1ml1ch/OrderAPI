using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderAPI.Controllers;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.UseCases.AddToCart;
using OrderAPI.Domain.UseCases.BookCart;
using OrderAPI.Domain.UseCases.GetCart;
using OrderAPI.Domain.UseCases.RemoveFromCart;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Domain.Enums;

namespace OrderAPI.Tests.Controllers;

public class CartsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly CartsController _sut;

    public CartsControllerTests()
    {
        _sut = new CartsController(_mediatorMock.Object);
    }

    private void SetUserClaims(Guid userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())]);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    private CartModel BuildCart(Guid cartId)
        => new()
        {
            Id = cartId,
            EventId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = ECurrency.USD,
            Status = EOrderStatus.Pending,
            Items = []
        };

    [Fact]
    public async Task GetCart_ReturnsOk_WithCartModel()
    {
        var cartId = Guid.NewGuid();
        var expected = BuildCart(cartId);
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCartRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetCart(cartId, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task AddToCart_ReturnsOk_WithCartModel()
    {
        var cartId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetUserClaims(userId);
        var body = new AddToCartRequestBody
        {
            EventId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            SeatId = Guid.NewGuid(),
            Price = 50m,
            Currency = ECurrency.USD
        };
        var expected = BuildCart(cartId);
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddToCartRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.AddToCart(cartId, body, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _mediatorMock.Verify(m => m.Send(It.Is<AddToCartRequest>(r =>
            r.CartId == cartId && r.CustomerId == userId && r.Body == body),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddToCart_ReturnsUnauthorized_WhenUserIdClaimMissing()
    {
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _sut.AddToCart(Guid.NewGuid(),
            new AddToCartRequestBody { EventId = Guid.NewGuid(), SectionId = Guid.NewGuid(), SeatId = Guid.NewGuid() },
            default);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RemoveFromCart_ReturnsNoContent()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoveFromCartRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.RemoveFromCart(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), default);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task BookCart_ReturnsOk_WithBookCartResponseModel()
    {
        var cartId = Guid.NewGuid();
        var expected = new BookCartResponseModel { PaymentId = Guid.NewGuid() };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BookCartRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.BookCart(cartId, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _mediatorMock.Verify(m => m.Send(It.Is<BookCartRequest>(r => r.CartId == cartId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
