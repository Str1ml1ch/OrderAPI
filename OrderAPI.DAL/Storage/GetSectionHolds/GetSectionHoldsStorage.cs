using Homework.Ticketing.System.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Core.Enums;
using OrderAPI.Core.Models;
using OrderAPI.DAL.Storage.Filters;

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
            var query = _context.SectionHolds
                .FilterByOrderId(orderId)
                .FilterBySectionId(sectionId)
                .FilterByStatus(status);

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
