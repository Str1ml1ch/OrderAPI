using Moq;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.Domain.Storage.GetOrderById;
using OrderAPI.Domain.Exceptions;
using OrderAPI.Domain.UseCases.GetCart;
using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Tests.UseCases;

public class GetCartRequestHandlerTests
{
    private readonly Mock<IGetOrderByIdStorage> _orderStorageMock = new();
    private readonly GetCartRequestHandler _sut;

    public GetCartRequestHandlerTests()
    {
        _sut = new GetCartRequestHandler(_orderStorageMock.Object);
    }

    [Fact]
    public async Task Handle_ThrowsOrderNotFoundException_WhenOrderDoesNotExist()
    {
        var cartId = Guid.NewGuid();
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDetailModel?)null);

        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _sut.Handle(new GetCartRequest { CartId = cartId }, default));
    }

    [Fact]
    public async Task Handle_ReturnsCartModel_WithMappedItems()
    {
        var cartId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var order = new OrderDetailModel
        {
            Id = cartId,
            CustomerId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            TotalAmount = 150m,
            Currency = ECurrency.USD,
            OrderStatus = EOrderStatus.Pending,
            Items =
            [
                new OrderItemModel
                {
                    Id = Guid.NewGuid(),
                    OrderId = cartId,
                    SectionId = sectionId,
                    SeatId = seatId,
                    Price = 150m,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ],
            SeatHolds = []
        };
        _orderStorageMock
            .Setup(s => s.GetOrderByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.Handle(new GetCartRequest { CartId = cartId }, default);

        Assert.Equal(cartId, result.Id);
        Assert.Equal(EOrderStatus.Pending, result.Status);
        Assert.Single(result.Items);
        Assert.Equal(seatId, result.Items[0].SeatId);
        Assert.Equal(sectionId, result.Items[0].SectionId);
        Assert.Equal(150m, result.Items[0].Price);
    }
}
