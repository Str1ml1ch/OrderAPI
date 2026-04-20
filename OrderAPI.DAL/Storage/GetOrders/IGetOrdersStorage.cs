using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Core.Enums;
using OrderAPI.Core.Models;

namespace OrderAPI.DAL.Storage.GetOrders
{
    public interface IGetOrdersStorage
    {
        Task<ResultModel<List<OrderModel>>> GetAsync(
            int page,
            int pageSize,
            Guid? customerId,
            Guid? eventId,
            EOrderStatus? status,
            CancellationToken ct);
    }
}
