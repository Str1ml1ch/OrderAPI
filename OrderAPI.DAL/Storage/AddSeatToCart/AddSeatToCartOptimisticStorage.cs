using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderAPI.DAL.Entities;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Exceptions;
using OrderAPI.Domain.Storage.AddSeatToCart;

namespace OrderAPI.DAL.Storage.AddSeatToCart
{
    public class AddSeatToCartOptimisticStorage : IAddSeatToCartStorage
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<AddSeatToCartOptimisticStorage> _logger;

        public AddSeatToCartOptimisticStorage(OrderDbContext context, ILogger<AddSeatToCartOptimisticStorage> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateSeatHoldAsync(Guid orderId, Guid seatId, DateTimeOffset holdExpirationTime, CancellationToken ct)
        {
            var isSold = await _context.OrderItems
                .Where(oi => oi.SeatId == seatId
                             && oi.Order.OrderStatus == EOrderStatus.Confirmed)
                .AnyAsync(ct);

            if (isSold)
            {
                _logger.LogWarning("Optimistic check: seat {SeatId} is already sold.", seatId);
                throw new SeatAlreadyBookedException(seatId);
            }

            var hold = new SeatHold
            {
                Id = Guid.NewGuid(),
                OderId = orderId,
                SeatId = seatId,
                SeatHoldStatus = ESeatSectionHoldStatus.Held,
                HoldExpirationTime = holdExpirationTime,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.SeatHolds.Add(hold);

            try
            {
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Optimistic: seat hold created for seat {SeatId}, order {OrderId}.", seatId, orderId);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sqlEx
                      && (sqlEx.Number == SqlErrorNumbers.DuplicateKeyInUniqueIndex
                          || sqlEx.Number == SqlErrorNumbers.UniqueConstraintViolation))
            {
                _logger.LogWarning("Optimistic conflict: unique index violation for seat {SeatId}. SQL error {ErrorNumber}.",
                    seatId, sqlEx.Number);
                throw new SeatAlreadyBookedException(seatId);
            }
        }
    }
}
