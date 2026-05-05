using Moq;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Storage.RemoveOrderItem;
using OrderAPI.Domain.Storage.RemoveSeatHold;
using OrderAPI.Domain.Storage.UpdateOrder;
using OrderAPI.Domain.Exceptions;
using OrderAPI.Domain.UseCases.RemoveFromCart;
using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Tests.UseCases;

public class RemoveFromCartRequestHandlerTests
{
    private readonly Mock<IGetOrderByIdStorage> _orderStorageMock = new();
    private readonly Mock<IRemoveOrderItemStorage> _removeItemMock = new();
    private readonly Mock<IRemoveSeatHoldStorage> _removeHoldMock = new();
    private readonly Mock<IUpdateOrderStorage> _updateOrderMock = new();
    private readonly RemoveFromCartRequestHandler _sut;

    public RemoveFromCartRequestHandlerTests()
    {
        _sut = new RemoveFromCartRequestHandler(
            _orderStorageMock.Object,
            _removeItemMock.Object,
            _removeHoldMock.Object,
            _updateOrderMock.Object);
    }

    [Fact]
    public async Task Handle_ThrowsOrderNotFoundException_WhenOrderDoesNotExist()
    {
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDetailModel?)null);

        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _sut.Handle(new RemoveFromCartRequest { CartId = Guid.NewGuid(), SeatId = Guid.NewGuid() }, default));
    }

    [Fact]
    public async Task Handle_ThrowsCartItemNotFoundException_WhenSeatNotInCart()
    {
        var cartId = Guid.NewGuid();
        var order = new OrderDetailModel
        {
            Id = cartId, CustomerId = Guid.NewGuid(), EventId = Guid.NewGuid(),
            TotalAmount = 0, Currency = ECurrency.USD,
            OrderStatus = EOrderStatus.Pending,
            Items = [],
            SeatHolds = []
        };
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<CartItemNotFoundException>(() =>
            _sut.Handle(new RemoveFromCartRequest { CartId = cartId, SeatId = Guid.NewGuid() }, default));
    }

    [Fact]
    public async Task Handle_RemovesItemAndHoldAndUpdatesTotal()
    {
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var holdId = Guid.NewGuid();
        var order = new OrderDetailModel
        {
            Id = cartId, CustomerId = Guid.NewGuid(), EventId = Guid.NewGuid(),
            TotalAmount = 150m, Currency = ECurrency.USD,
            OrderStatus = EOrderStatus.Pending,
            Items =
            [
                new OrderItemModel { Id = itemId, OrderId = cartId, SectionId = Guid.NewGuid(), SeatId = seatId, Price = 150m, CreatedAt = DateTimeOffset.UtcNow }
            ],
            SeatHolds =
            [
                new SeatHoldModel { Id = holdId, SeatId = seatId, OderId = cartId, SeatHoldStatus = ESeatSectionHoldStatus.Held }
            ]
        };
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _removeItemMock
            .Setup(s => s.RemoveByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _removeHoldMock
            .Setup(s => s.RemoveByIdAsync(holdId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _updateOrderMock
            .Setup(s => s.UpdateAmountAsync(cartId, 0m, ECurrency.USD, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(new RemoveFromCartRequest { CartId = cartId, SeatId = seatId }, default);

        Assert.True(result);
        _removeItemMock.Verify(s => s.RemoveByIdAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
        _removeHoldMock.Verify(s => s.RemoveByIdAsync(holdId, It.IsAny<CancellationToken>()), Times.Once);
        _updateOrderMock.Verify(s => s.UpdateAmountAsync(cartId, 0m, ECurrency.USD, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RemovesItemOnly_WhenNoHoldExists()
    {
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var order = new OrderDetailModel
        {
            Id = cartId, CustomerId = Guid.NewGuid(), EventId = Guid.NewGuid(),
            TotalAmount = 100m, Currency = ECurrency.USD,
            OrderStatus = EOrderStatus.Pending,
            Items =
            [
                new OrderItemModel { Id = itemId, OrderId = cartId, SectionId = Guid.NewGuid(), SeatId = seatId, Price = 100m, CreatedAt = DateTimeOffset.UtcNow }
            ],
            SeatHolds = []
        };
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _removeItemMock
            .Setup(s => s.RemoveByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _updateOrderMock
            .Setup(s => s.UpdateAmountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(new RemoveFromCartRequest { CartId = cartId, SeatId = seatId }, default);

        _removeHoldMock.Verify(s => s.RemoveByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
