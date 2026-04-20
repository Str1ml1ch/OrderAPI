using OrderAPI.DAL.Entities;
using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.Filters
{
    public static class SectionHoldQueryExtensions
    {
        public static IQueryable<SectionHold> FilterByOrderId(this IQueryable<SectionHold> query, Guid? orderId)
        {
            if (orderId.HasValue)
                query = query.Where(sh => sh.OderId == orderId.Value);
            return query;
        }

        public static IQueryable<SectionHold> FilterBySectionId(this IQueryable<SectionHold> query, Guid? sectionId)
        {
            if (sectionId.HasValue)
                query = query.Where(sh => sh.SectionId == sectionId.Value);
            return query;
        }

        public static IQueryable<SectionHold> FilterByStatus(this IQueryable<SectionHold> query, ESeatSectionHoldStatus? status)
        {
            if (status.HasValue)
                query = query.Where(sh => sh.SectionHoldStatus == status.Value);
            return query;
        }
    }
}
