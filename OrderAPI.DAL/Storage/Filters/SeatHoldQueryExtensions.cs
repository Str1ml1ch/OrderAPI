using OrderAPI.DAL.Entities;
using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.Filters
{
    public static class SeatHoldQueryExtensions
    {
        public static IQueryable<SeatHold> FilterByOrderId(this IQueryable<SeatHold> query, Guid? orderId)
        {
            if (orderId.HasValue)
                query = query.Where(sh => sh.OderId == orderId.Value);
            return query;
        }

        public static IQueryable<SeatHold> FilterByStatus(this IQueryable<SeatHold> query, ESeatSectionHoldStatus? status)
        {
            if (status.HasValue)
                query = query.Where(sh => sh.SeatHoldStatus == status.Value);
            return query;
        }
    }
}
