namespace OrderAPI.Domain.Storage.CreateSectionHold
{
    public interface ICreateSectionHoldStorage
    {
        Task<Guid> CreateAsync(Guid orderId, Guid sectionId, int quantity, DateTimeOffset holdExpirationTime, CancellationToken ct);
    }
}
