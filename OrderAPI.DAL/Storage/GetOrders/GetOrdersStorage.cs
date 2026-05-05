using OrderAPI.Domain.Storage.GetOrders;
using Homework.Ticketing.System.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.DAL.Specifications.Orders;

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
            var query = _context.Orders.AsQueryable();
            if (customerId.HasValue)
                query = query.Where(new OrderByCustomerIdSpecification(customerId.Value).ToExpression());
            if (eventId.HasValue)
                query = query.Where(new OrderByEventIdSpecification(eventId.Value).ToExpression());
            if (status.HasValue)
                query = query.Where(new OrderByStatusSpecification(status.Value).ToExpression());

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
