using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.Storage.GetSeatStatuses
{
    public interface IGetSeatStatusesStorage
    {
        Task<Dictionary<Guid, OrderAPI.Domain.Enums.ESeatStatus>> GetAsync(
            IEnumerable<Guid> seatIds,
            CancellationToken ct);
    }
}
