using OrderAPI.Domain.Storage.GetSectionHolds;
using Homework.Ticketing.System.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.DAL.Specifications.SectionHolds;

namespace OrderAPI.DAL.Storage.GetSectionHolds
{
    public class GetSectionHoldsStorage : IGetSectionHoldsStorage
    {
        private readonly OrderDbContext _context;

        public GetSectionHoldsStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<ResultModel<List<SectionHoldModel>>> GetAsync(
            int page,
            int pageSize,
            Guid? orderId,
            Guid? sectionId,
            ESeatSectionHoldStatus? status,
            CancellationToken ct)
        {
            var query = _context.SectionHolds.AsQueryable();
            if (orderId.HasValue)
                query = query.Where(new SectionHoldByOrderIdSpecification(orderId.Value).ToExpression());
            if (sectionId.HasValue)
                query = query.Where(new SectionHoldBySectionIdSpecification(sectionId.Value).ToExpression());
            if (status.HasValue)
                query = query.Where(new SectionHoldByStatusSpecification(status.Value).ToExpression());

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(sh => sh.HoldExpirationTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sh => new SectionHoldModel
                {
                    Id = sh.Id,
                    SectionId = sh.SectionId,
                    OderId = sh.OderId,
                    SectionHoldStatus = sh.SectionHoldStatus,
                    HoldExpirationTime = sh.HoldExpirationTime,
                    Quantity = sh.Quantity,
                    CreatedAt = sh.CreatedAt,
                    UpdatedAt = sh.UpdatedAt
                })
                .ToListAsync(ct);

            return new ResultModel<List<SectionHoldModel>> { Data = items, Count = totalCount };
        }
    }
}
