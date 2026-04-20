using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.CreateSeatHold
{
    public interface ICreateSeatHoldStorage
    {
        Task<Guid> CreateAsync(Guid orderId, Guid seatId, DateTimeOffset holdExpirationTime, CancellationToken ct);
    }
}
