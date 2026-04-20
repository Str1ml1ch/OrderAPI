namespace OrderAPI.DAL.Storage.RemoveOrder
{
    public interface IRemoveOrderStorage
    {
        Task RemoveByIdAsync(Guid id, CancellationToken ct);
    }
}
