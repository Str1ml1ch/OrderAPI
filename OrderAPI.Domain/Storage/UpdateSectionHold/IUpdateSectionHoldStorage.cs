using OrderAPI.Domain.Enums;

namespace OrderAPI.Domain.Storage.UpdateSectionHold
{
    public interface IUpdateSectionHoldStorage
    {
        Task UpdateStatusAsync(Guid id, ESeatSectionHoldStatus status, CancellationToken ct);
    }
}
