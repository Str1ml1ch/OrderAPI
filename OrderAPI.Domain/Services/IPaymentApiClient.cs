using Homework.Ticketing.System.Shared.Enums;

namespace OrderAPI.Domain.Services
{
    public interface IPaymentApiClient
    {
        Task<Guid> CreatePaymentAsync(Guid orderId, decimal amount, ECurrency currency, CancellationToken ct);
    }
}
