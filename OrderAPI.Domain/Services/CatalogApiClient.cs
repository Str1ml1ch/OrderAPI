using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Core.Models;
using System.Net.Http.Json;

namespace OrderAPI.Domain.Services
{
    public class CatalogApiClient : ICatalogApiClient
    {
        private readonly IHttpClientFactory _factory;

        public CatalogApiClient(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<ResultModel<List<CatalogSeatModel>>?> GetEventSectionSeatsAsync(
            Guid eventId,
            Guid sectionId,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var client = _factory.CreateClient("CatalogApi");
            var url = $"/api/events/{eventId}/sections/{sectionId}/seats?page={page}&pageSize={pageSize}";
            return await client.GetFromJsonAsync<ResultModel<List<CatalogSeatModel>>>(url, ct);
        }
    }
}
