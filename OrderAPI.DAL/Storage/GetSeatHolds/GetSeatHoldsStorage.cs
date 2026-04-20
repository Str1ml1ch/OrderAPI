using Homework.Ticketing.System.Shared.Models;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Core.Enums;
using OrderAPI.Core.Models;
using OrderAPI.DAL.Storage.Filters;

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
            var query = _context.SeatHolds
                .FilterByOrderId(orderId)
                .FilterByStatus(status);

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
