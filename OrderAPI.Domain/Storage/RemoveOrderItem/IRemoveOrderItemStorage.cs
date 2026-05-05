namespace OrderAPI.Domain.Storage.RemoveOrderItem
{
    public interface IRemoveOrderItemStorage
    {
        Task RemoveByIdAsync(Guid id, CancellationToken ct);
        Task RemoveAllByOrderIdAsync(Guid orderId, CancellationToken ct);
    }
}
