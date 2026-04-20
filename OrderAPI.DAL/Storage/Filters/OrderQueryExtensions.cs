using OrderAPI.DAL.Entities;
using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.Filters
{
    public static class OrderQueryExtensions
    {
        public static IQueryable<Order> FilterByCustomerId(this IQueryable<Order> query, Guid? customerId)
        {
            if (customerId.HasValue)
                query = query.Where(o => o.CustomerId == customerId.Value);
            return query;
        }

        public static IQueryable<Order> FilterByEventId(this IQueryable<Order> query, Guid? eventId)
        {
            if (eventId.HasValue)
                query = query.Where(o => o.EventId == eventId.Value);
            return query;
        }

        public static IQueryable<Order> FilterByStatus(this IQueryable<Order> query, EOrderStatus? status)
        {
            if (status.HasValue)
                query = query.Where(o => o.OrderStatus == status.Value);
            return query;
        }
    }
}
