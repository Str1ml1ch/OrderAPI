using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Domain.Enums;

namespace OrderAPI.Domain.Storage.CreateOrder
{
    public interface ICreateOrderStorage
    {
        Task<Guid> CreateAsync(Guid customerId, Guid eventId, decimal totalAmount, ECurrency currency, EOrderStatus status, CancellationToken ct);
        Task<Guid> CreateWithIdAsync(Guid id, Guid customerId, Guid eventId, decimal totalAmount, ECurrency currency, EOrderStatus status, CancellationToken ct);
    }
}
