using Moq;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.Storage.CreateOrder;
using OrderAPI.Domain.Storage.CreateOrderItem;
using OrderAPI.Domain.Storage.CreateSeatHold;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Storage.UpdateOrder;
using OrderAPI.Domain.UseCases.AddToCart;
using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Tests.UseCases;

public class AddToCartRequestHandlerTests
{
    private readonly Mock<IGetOrderByIdStorage> _orderStorageMock = new();
    private readonly Mock<ICreateOrderStorage> _createOrderMock = new();
    private readonly Mock<ICreateOrderItemStorage> _createItemMock = new();
    private readonly Mock<ICreateSeatHoldStorage> _createHoldMock = new();
    private readonly Mock<IUpdateOrderStorage> _updateOrderMock = new();
    private readonly AddToCartRequestHandler _sut;

    public AddToCartRequestHandlerTests()
    {
        _sut = new AddToCartRequestHandler(
            _orderStorageMock.Object,
            _createOrderMock.Object,
            _createItemMock.Object,
            _createHoldMock.Object,
            _updateOrderMock.Object);
    }

    private AddToCartRequest BuildRequest(Guid? cartId = null)
    {
        return new AddToCartRequest
        {
            CartId = cartId ?? Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Body = new AddToCartRequestBody
            {
                EventId = Guid.NewGuid(),
                SectionId = Guid.NewGuid(),
                SeatId = Guid.NewGuid(),
                Price = 75m,
                Currency = ECurrency.USD
            }
        };
    }

    private OrderDetailModel BuildOrderDetail(Guid cartId, Guid eventId, decimal total = 0)
        => new()
        {
            Id = cartId,
            CustomerId = Guid.NewGuid(),
            EventId = eventId,
            TotalAmount = total,
            Currency = ECurrency.USD,
            OrderStatus = EOrderStatus.Pending,
            Items = [],
            SeatHolds = []
        };

    [Fact]
    public async Task Handle_CreatesNewOrder_WhenOrderNotFound()
    {
        var request = BuildRequest();
        var createdOrder = BuildOrderDetail(request.CartId, request.Body.EventId);
        _orderStorageMock
            .SetupSequence(s => s.GetOrderByIdAsync(request.CartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDetailModel?)null)
            .ReturnsAsync(createdOrder)  // second call after create
            .ReturnsAsync(createdOrder); // third call for final result

        _createOrderMock
            .Setup(s => s.CreateWithIdAsync(
                request.CartId, request.CustomerId, request.Body.EventId,
                0, request.Body.Currency, EOrderStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request.CartId);

        _createItemMock
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _createHoldMock
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _updateOrderMock
            .Setup(s => s.UpdateAmountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(request, default);

        _createOrderMock.Verify(s => s.CreateWithIdAsync(
            request.CartId, request.CustomerId, request.Body.EventId,
            0, request.Body.Currency, EOrderStatus.Pending, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotCreateOrder_WhenOrderAlreadyExists()
    {
        var request = BuildRequest();
        var existingOrder = BuildOrderDetail(request.CartId, request.Body.EventId);
        _orderStorageMock
            .SetupSequence(s => s.GetOrderByIdAsync(request.CartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder)
            .ReturnsAsync(existingOrder);

        _createItemMock
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _createHoldMock
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _updateOrderMock
            .Setup(s => s.UpdateAmountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(request, default);

        _createOrderMock.Verify(s => s.CreateWithIdAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<decimal>(), It.IsAny<ECurrency>(), It.IsAny<EOrderStatus>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CreatesItemAndHold_Always()
    {
        var request = BuildRequest();
        var existingOrder = BuildOrderDetail(request.CartId, request.Body.EventId);
        _orderStorageMock
            .SetupSequence(s => s.GetOrderByIdAsync(request.CartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder)
            .ReturnsAsync(existingOrder);

        _createItemMock
            .Setup(s => s.CreateAsync(request.CartId, request.Body.SectionId, request.Body.SeatId, request.Body.Price, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _createHoldMock
            .Setup(s => s.CreateAsync(request.CartId, request.Body.SeatId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _updateOrderMock
            .Setup(s => s.UpdateAmountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<ECurrency>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(request, default);

        _createItemMock.Verify(s => s.CreateAsync(
            request.CartId, request.Body.SectionId, request.Body.SeatId, request.Body.Price,
            It.IsAny<CancellationToken>()), Times.Once);
        _createHoldMock.Verify(s => s.CreateAsync(
            request.CartId, request.Body.SeatId, It.IsAny<DateTimeOffset>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
