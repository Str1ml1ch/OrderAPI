using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Domain.Enums;

namespace OrderAPI.Domain.Storage.UpdateOrder
{
    public interface IUpdateOrderStorage
    {
        Task UpdateStatusAsync(Guid id, EOrderStatus status, CancellationToken ct);
        Task UpdateAmountAsync(Guid id, decimal totalAmount, ECurrency currency, CancellationToken ct);
    }
}
