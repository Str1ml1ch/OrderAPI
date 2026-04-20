using Homework.Ticketing.System.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Core.Enums;
using OrderAPI.Core.Models;
using OrderAPI.DAL.Storage.Filters;

namespace OrderAPI.DAL.Storage.GetOrders
{
    public class GetOrdersStorage : IGetOrdersStorage
    {
        private readonly OrderDbContext _context;

        public GetOrdersStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<ResultModel<List<OrderModel>>> GetAsync(
            int page,
            int pageSize,
            Guid? customerId,
            Guid? eventId,
            EOrderStatus? status,
            CancellationToken ct)
        {
            var query = _context.Orders
                .FilterByCustomerId(customerId)
                .FilterByEventId(eventId)
                .FilterByStatus(status);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderModel
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    EventId = o.EventId,
                    TotalAmount = o.TotalAmount,
                    Currency = o.Currency,
                    OrderStatus = o.OrderStatus,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync(ct);

            return new ResultModel<List<OrderModel>> { Data = items, Count = totalCount };
        }
    }
}
