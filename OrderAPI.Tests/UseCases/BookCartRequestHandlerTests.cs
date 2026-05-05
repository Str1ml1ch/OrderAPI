using Moq;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Storage.UpdateOrder;
using OrderAPI.Domain.Storage.UpdateSeatHold;
using OrderAPI.Domain.Exceptions;
using OrderAPI.Domain.Services;
using OrderAPI.Domain.UseCases.BookCart;
using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Tests.UseCases;

public class BookCartRequestHandlerTests
{
    private readonly Mock<IGetOrderByIdStorage> _orderStorageMock = new();
    private readonly Mock<IUpdateOrderStorage> _updateOrderMock = new();
    private readonly Mock<IUpdateSeatHoldStorage> _updateHoldMock = new();
    private readonly Mock<IPaymentApiClient> _paymentMock = new();
    private readonly BookCartRequestHandler _sut;

    public BookCartRequestHandlerTests()
    {
        _sut = new BookCartRequestHandler(
            _orderStorageMock.Object,
            _updateOrderMock.Object,
            _updateHoldMock.Object,
            _paymentMock.Object);
    }

    private OrderDetailModel BuildOrder(Guid id, EOrderStatus status = EOrderStatus.Pending)
        => new()
        {
            Id = id,
            CustomerId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            TotalAmount = 200m,
            Currency = ECurrency.USD,
            OrderStatus = status,
            Items = [],
            SeatHolds = []
        };

    [Fact]
    public async Task Handle_ThrowsOrderNotFoundException_WhenOrderDoesNotExist()
    {
        var cartId = Guid.NewGuid();
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDetailModel?)null);

        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _sut.Handle(new BookCartRequest { CartId = cartId }, default));
    }

    [Fact]
    public async Task Handle_ThrowsOrderNotPendingException_WhenOrderIsNotPending()
    {
        var cartId = Guid.NewGuid();
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOrder(cartId, EOrderStatus.Confirmed));

        await Assert.ThrowsAsync<OrderNotPendingException>(() =>
            _sut.Handle(new BookCartRequest { CartId = cartId }, default));
    }

    [Fact]
    public async Task Handle_ConfirmsOrderAndHolds_AndCreatesPayment()
    {
        var cartId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var order = BuildOrder(cartId);

        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _updateHoldMock
            .Setup(s => s.UpdateAllByOrderIdAsync(cartId, ESeatSectionHoldStatus.Confirmed, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _updateOrderMock
            .Setup(s => s.UpdateStatusAsync(cartId, EOrderStatus.Confirmed, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _paymentMock
            .Setup(s => s.CreatePaymentAsync(cartId, order.TotalAmount, order.Currency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentId);

        var result = await _sut.Handle(new BookCartRequest { CartId = cartId }, default);

        Assert.Equal(paymentId, result.PaymentId);
        _updateHoldMock.Verify(s => s.UpdateAllByOrderIdAsync(cartId, ESeatSectionHoldStatus.Confirmed, It.IsAny<CancellationToken>()), Times.Once);
        _updateOrderMock.Verify(s => s.UpdateStatusAsync(cartId, EOrderStatus.Confirmed, It.IsAny<CancellationToken>()), Times.Once);
    }
}
