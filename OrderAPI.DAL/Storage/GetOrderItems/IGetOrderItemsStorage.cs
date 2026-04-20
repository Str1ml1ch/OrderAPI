using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Core.Models;

namespace OrderAPI.DAL.Storage.GetOrderItems
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
