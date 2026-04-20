using Homework.Ticketing.System.Shared.Enums;
using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.UpdateOrder
{
    public interface IUpdateOrderStorage
    {
        Task UpdateStatusAsync(Guid id, EOrderStatus status, CancellationToken ct);
        Task UpdateAmountAsync(Guid id, decimal totalAmount, ECurrency currency, CancellationToken ct);
    }
}
