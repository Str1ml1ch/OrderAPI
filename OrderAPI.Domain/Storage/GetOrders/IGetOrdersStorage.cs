using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.Storage.GetOrders
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
