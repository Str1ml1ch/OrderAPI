using OrderAPI.Core.Models;

namespace OrderAPI.DAL.Storage.GetOrderById
{
    public interface IGetOrderByIdStorage
    {
        Task<OrderDetailModel?> GetOrderByIdAsync(Guid id, CancellationToken ct);
        Task<bool> IsOrderExistsAsync(Guid id, CancellationToken ct);
    }
}
