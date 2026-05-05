using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.Storage.GetOrderItems
{
    public interface IGetOrderItemsStorage
    {
        Task<ResultModel<List<OrderItemModel>>> GetAsync(
            int page,
            int pageSize,
            Guid orderId,
            CancellationToken ct);
    }
}
