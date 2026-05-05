using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderAPI.Controllers;
using OrderAPI.Domain.UseCases.GetSeatStatuses;

namespace OrderAPI.Tests.Controllers;

public class SeatsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly SeatsController _sut;

    public SeatsControllerTests()
    {
        _sut = new SeatsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetStatuses_ReturnsOk_WithStatusDictionary()
    {
        var seatId1 = Guid.NewGuid();
        var seatId2 = Guid.NewGuid();
        var expected = new Dictionary<Guid, int>
        {
            { seatId1, 0 },
            { seatId2, 1 }
        };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetSeatStatusesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetStatuses([seatId1, seatId2], default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetSeatStatusesRequest>(r => r.SeatIds.SequenceEqual(new[] { seatId1, seatId2 })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatuses_ReturnsEmptyDictionary_WhenNoSeatIds()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetSeatStatusesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _sut.GetStatuses([], default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Dictionary<Guid, int>>(ok.Value);
    }
}
