using OrderAPI.Domain.Enums;

namespace OrderAPI.Domain.Storage.UpdateSeatHold
{
    public interface IUpdateSeatHoldStorage
    {
        Task UpdateStatusAsync(Guid id, ESeatSectionHoldStatus status, CancellationToken ct);
        Task UpdateAllByOrderIdAsync(Guid orderId, ESeatSectionHoldStatus status, CancellationToken ct);
    }
}
