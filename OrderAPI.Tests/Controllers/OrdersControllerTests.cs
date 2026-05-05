using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderAPI.Controllers;
using OrderAPI.Domain.UseCases.CancelOrder;

namespace OrderAPI.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Cancel_ReturnsNoContent_WhenCancelSucceeds()
    {
        var orderId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Cancel(orderId, default);

        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<CancelOrderRequest>(r => r.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
