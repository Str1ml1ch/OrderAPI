using OrderAPI.Domain.Storage.GetSeatStatuses;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Domain.Enums;

namespace OrderAPI.DAL.Storage.GetSeatStatuses
{
    public class GetSeatStatusesStorage : IGetSeatStatusesStorage
    {
        private readonly OrderDbContext _context;

        public GetSeatStatusesStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<Guid, ESeatStatus>> GetAsync(
            IEnumerable<Guid> seatIds,
            CancellationToken ct)
        {
            var seatIdList = seatIds.ToList();
            var now = DateTimeOffset.UtcNow;

            var soldSeatIds = await _context.OrderItems
                .Where(oi => oi.SeatId.HasValue
                    && seatIdList.Contains(oi.SeatId.Value)
                    && oi.Order.OrderStatus == EOrderStatus.Confirmed)
                .Select(oi => oi.SeatId!.Value)
                .Distinct()
                .ToListAsync(ct);

            var reservedSeatIds = await _context.SeatHolds
                .Where(sh => sh.SeatId.HasValue
                    && seatIdList.Contains(sh.SeatId.Value)
                    && sh.SeatHoldStatus == ESeatSectionHoldStatus.Held
                    && sh.HoldExpirationTime > now)
                .Select(sh => sh.SeatId!.Value)
                .Distinct()
                .ToListAsync(ct);

            return seatIdList.ToDictionary(
                id => id,
                id =>
                {
                    if (soldSeatIds.Contains(id)) return ESeatStatus.Sold;
                    if (reservedSeatIds.Contains(id)) return ESeatStatus.Reserved;
                    return ESeatStatus.Available;
                });
        }
    }
}
