using OrderAPI.Domain.Enums;

namespace OrderAPI.Domain.Storage.CreateSeatHold
{
    public interface ICreateSeatHoldStorage
    {
        Task<Guid> CreateAsync(Guid orderId, Guid seatId, DateTimeOffset holdExpirationTime, CancellationToken ct);
    }
}
