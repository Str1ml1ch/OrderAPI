using MediatR;
using OrderAPI.Domain.Storage.GetSeatStatuses;

namespace OrderAPI.Domain.UseCases.GetSeatStatuses
{
    public class GetSeatStatusesRequestHandler : IRequestHandler<GetSeatStatusesRequest, Dictionary<Guid, int>>
    {
        private readonly IGetSeatStatusesStorage _storage;

        public GetSeatStatusesRequestHandler(IGetSeatStatusesStorage storage)
        {
            _storage = storage;
        }

        public async Task<Dictionary<Guid, int>> Handle(GetSeatStatusesRequest request, CancellationToken cancellationToken)
        {
            var statuses = await _storage.GetAsync(request.SeatIds, cancellationToken);
            return statuses.ToDictionary(kvp => kvp.Key, kvp => (int)kvp.Value);
        }
    }
}
