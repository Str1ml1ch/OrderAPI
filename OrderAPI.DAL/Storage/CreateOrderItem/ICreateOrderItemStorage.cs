namespace OrderAPI.DAL.Storage.CreateOrderItem
{
    public interface ICreateOrderItemStorage
    {
        Task<Guid> CreateAsync(Guid orderId, Guid sectionId, Guid? seatId, decimal price, CancellationToken ct);
    }
}
