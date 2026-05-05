using Moq;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Storage.UpdateOrder;
using OrderAPI.Domain.Storage.UpdateSeatHold;
using OrderAPI.Domain.Exceptions;
using OrderAPI.Domain.UseCases.CancelOrder;

namespace OrderAPI.Tests.UseCases;

public class CancelOrderRequestHandlerTests
{
    private readonly Mock<IGetOrderByIdStorage> _orderStorageMock = new();
    private readonly Mock<IUpdateOrderStorage> _updateOrderMock = new();
    private readonly Mock<IUpdateSeatHoldStorage> _updateHoldMock = new();
    private readonly CancelOrderRequestHandler _sut;

    public CancelOrderRequestHandlerTests()
    {
        _sut = new CancelOrderRequestHandler(
            _orderStorageMock.Object,
            _updateOrderMock.Object,
            _updateHoldMock.Object);
    }

    [Fact]
    public async Task Handle_ThrowsOrderNotFoundException_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        _orderStorageMock
            .Setup(s => s.IsOrderExistsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _sut.Handle(new CancelOrderRequest { OrderId = orderId }, default));
    }

    [Fact]
    public async Task Handle_ReleasesHoldsAndCancelsOrder_WhenOrderExists()
    {
        var orderId = Guid.NewGuid();
        _orderStorageMock
            .Setup(s => s.IsOrderExistsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _updateHoldMock
            .Setup(s => s.UpdateAllByOrderIdAsync(orderId, ESeatSectionHoldStatus.Released, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _updateOrderMock
            .Setup(s => s.UpdateStatusAsync(orderId, EOrderStatus.Cancelled, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(new CancelOrderRequest { OrderId = orderId }, default);

        Assert.True(result);
        _updateHoldMock.Verify(s => s.UpdateAllByOrderIdAsync(orderId, ESeatSectionHoldStatus.Released, It.IsAny<CancellationToken>()), Times.Once);
        _updateOrderMock.Verify(s => s.UpdateStatusAsync(orderId, EOrderStatus.Cancelled, It.IsAny<CancellationToken>()), Times.Once);
    }
}
