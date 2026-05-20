using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderAPI.DAL.Entities;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Exceptions;
using OrderAPI.Domain.Storage.AddSeatToCart;
using System.Data;

namespace OrderAPI.DAL.Storage.AddSeatToCart
{
    public class AddSeatToCartPessimisticStorage : IAddSeatToCartStorage
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<AddSeatToCartPessimisticStorage> _logger;

        public AddSeatToCartPessimisticStorage(OrderDbContext context, ILogger<AddSeatToCartPessimisticStorage> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateSeatHoldAsync(Guid orderId, Guid seatId, DateTimeOffset holdExpirationTime, CancellationToken ct)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                try
                {
                    var activeSeatHold = await _context.SeatHolds
                        .Where(sh => sh.SeatId == seatId
                                     && (sh.SeatHoldStatus == ESeatSectionHoldStatus.Held
                                         || sh.SeatHoldStatus == ESeatSectionHoldStatus.Confirmed))
                        .AnyAsync(ct);

                    if (activeSeatHold)
                    {
                        _logger.LogWarning("Pessimistic check: seat {SeatId} is already held/confirmed.", seatId);
                        throw new SeatAlreadyBookedException(seatId);
                    }

                    var isSold = await _context.OrderItems
                        .Where(oi => oi.SeatId == seatId
                                     && oi.Order.OrderStatus == EOrderStatus.Confirmed)
                        .AnyAsync(ct);

                    if (isSold)
                    {
                        _logger.LogWarning("Pessimistic check: seat {SeatId} is already sold.", seatId);
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
                    await _context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    _logger.LogInformation("Pessimistic: seat hold created for seat {SeatId}, order {OrderId}.", seatId, orderId);
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }
    }
}
