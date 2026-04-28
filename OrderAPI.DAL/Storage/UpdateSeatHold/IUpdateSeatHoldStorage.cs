using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.UpdateSeatHold
{
    public interface IUpdateSeatHoldStorage
    {
        Task UpdateStatusAsync(Guid id, ESeatSectionHoldStatus status, CancellationToken ct);
        Task UpdateAllByOrderIdAsync(Guid orderId, ESeatSectionHoldStatus status, CancellationToken ct);
    }
}
