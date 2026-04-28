using MediatR;

namespace OrderAPI.Domain.UseCases.GetSeatStatuses
{
    public class GetSeatStatusesRequest : IRequest<Dictionary<Guid, int>>
    {
        public IEnumerable<Guid> SeatIds { get; set; } = [];
    }
}
