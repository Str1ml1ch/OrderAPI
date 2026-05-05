using OrderAPI.Domain.Storage.GetSeatHolds;
using Homework.Ticketing.System.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;
using OrderAPI.DAL.Specifications.SeatHolds;

namespace OrderAPI.DAL.Storage.GetSeatHolds
{
    public class GetSeatHoldsStorage : IGetSeatHoldsStorage
    {
        private readonly OrderDbContext _context;

        public GetSeatHoldsStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<ResultModel<List<SeatHoldModel>>> GetAsync(
            int page,
            int pageSize,
            Guid? orderId,
            ESeatSectionHoldStatus? status,
            CancellationToken ct)
        {
            var query = _context.SeatHolds.AsQueryable();
            if (orderId.HasValue)
                query = query.Where(new SeatHoldByOrderIdSpecification(orderId.Value).ToExpression());
            if (status.HasValue)
                query = query.Where(new SeatHoldByStatusSpecification(status.Value).ToExpression());

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(sh => sh.HoldExpirationTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sh => new SeatHoldModel
                {
                    Id = sh.Id,
                    SeatId = sh.SeatId,
                    OderId = sh.OderId,
                    SeatHoldStatus = sh.SeatHoldStatus,
                    HoldExpirationTime = sh.HoldExpirationTime,
                    CreatedAt = sh.CreatedAt,
                    UpdatedAt = sh.UpdatedAt
                })
                .ToListAsync(ct);

            return new ResultModel<List<SeatHoldModel>> { Data = items, Count = totalCount };
        }
    }
}
