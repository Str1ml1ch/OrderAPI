using Microsoft.EntityFrameworkCore;
using OrderAPI.Core.Models;

namespace OrderAPI.DAL.Storage.GetOrderById
{
    public class GetOrderByIdStorage : IGetOrderByIdStorage
    {
        private readonly OrderDbContext _context;

        public GetOrderByIdStorage(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDetailModel?> GetOrderByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Orders
                .Where(o => o.Id == id)
                .Select(o => new OrderDetailModel
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    EventId = o.EventId,
                    TotalAmount = o.TotalAmount,
                    Currency = o.Currency,
                    OrderStatus = o.OrderStatus,
                    Items = o.OrderItems.Select(oi => new OrderItemModel
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        SectionId = oi.SectionId,
                        SeatId = oi.SeatId,
                        Price = oi.Price,
                        CreatedAt = oi.CreatedAt,
                        UpdatedAt = oi.UpdatedAt
                    }).ToList(),
                    SeatHolds = o.SeatHolds.Select(sh => new SeatHoldModel
                    {
                        Id = sh.Id,
                        SeatId = sh.SeatId,
                        OderId = sh.OderId,
                        SeatHoldStatus = sh.SeatHoldStatus,
                        HoldExpirationTime = sh.HoldExpirationTime,
                        CreatedAt = sh.CreatedAt,
                        UpdatedAt = sh.UpdatedAt
                    }).ToList(),
                    SectionHolds = o.SectionHolds.Select(sh => new SectionHoldModel
                    {
                        Id = sh.Id,
                        SectionId = sh.SectionId,
                        OderId = sh.OderId,
                        SectionHoldStatus = sh.SectionHoldStatus,
                        HoldExpirationTime = sh.HoldExpirationTime,
                        Quantity = sh.Quantity,
                        CreatedAt = sh.CreatedAt,
                        UpdatedAt = sh.UpdatedAt
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> IsOrderExistsAsync(Guid id, CancellationToken ct)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id, ct);
        }
    }
}
