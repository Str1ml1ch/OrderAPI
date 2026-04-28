using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Core.Models;

namespace OrderAPI.DAL.Storage.GetSeatStatuses
{
    public interface IGetSeatStatusesStorage
    {
        Task<Dictionary<Guid, OrderAPI.Core.Enums.ESeatStatus>> GetAsync(
            IEnumerable<Guid> seatIds,
            CancellationToken ct);
    }
}
