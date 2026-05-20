namespace OrderAPI.Domain.Storage.AddSeatToCart
{
    public interface IAddSeatToCartStorage
    {
        Task CreateSeatHoldAsync(Guid orderId, Guid seatId, DateTimeOffset holdExpirationTime, CancellationToken ct);
    }
}
