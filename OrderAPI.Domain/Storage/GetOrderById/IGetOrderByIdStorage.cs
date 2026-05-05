using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.Storage.GetOrderById
{
    public interface IGetOrderByIdStorage
    {
        Task<OrderDetailModel?> GetOrderByIdAsync(Guid id, CancellationToken ct);
        Task<bool> IsOrderExistsAsync(Guid id, CancellationToken ct);
    }
}
