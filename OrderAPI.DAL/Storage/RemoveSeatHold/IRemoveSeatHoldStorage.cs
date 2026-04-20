namespace OrderAPI.DAL.Storage.RemoveSeatHold
{
    public interface IRemoveSeatHoldStorage
    {
        Task RemoveByIdAsync(Guid id, CancellationToken ct);
        Task RemoveAllByOrderIdAsync(Guid orderId, CancellationToken ct);
    }
}
