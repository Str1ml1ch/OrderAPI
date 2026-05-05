namespace OrderAPI.Domain.Storage.RemoveSectionHold
{
    public interface IRemoveSectionHoldStorage
    {
        Task RemoveByIdAsync(Guid id, CancellationToken ct);
        Task RemoveAllByOrderIdAsync(Guid orderId, CancellationToken ct);
    }
}
