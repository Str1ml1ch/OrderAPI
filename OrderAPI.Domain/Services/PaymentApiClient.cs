using Homework.Ticketing.System.Shared.Enums;
using System.Net.Http.Json;

namespace OrderAPI.Domain.Services
{
    public class PaymentApiClient : IPaymentApiClient
    {
        private readonly IHttpClientFactory _factory;

        public PaymentApiClient(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<Guid> CreatePaymentAsync(Guid orderId, decimal amount, ECurrency currency, CancellationToken ct)
        {
            var client = _factory.CreateClient("PaymentApi");
            var response = await client.PostAsJsonAsync("/api/payments", new
            {
                orderId,
                amount,
                currency = (int)currency
            }, ct);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>(cancellationToken: ct);
            return result!.PaymentId;
        }

        private class CreatePaymentResponse
        {
            public Guid PaymentId { get; set; }
        }
    }
}
