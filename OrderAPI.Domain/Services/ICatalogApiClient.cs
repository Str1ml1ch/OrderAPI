using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.Services
{
    public interface ICatalogApiClient
    {
        Task<ResultModel<List<CatalogSeatModel>>?> GetEventSectionSeatsAsync(
            Guid eventId,
            Guid sectionId,
            int page,
            int pageSize,
            CancellationToken ct);
    }
}
