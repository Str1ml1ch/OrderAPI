using OrderAPI.Core.Enums;

namespace OrderAPI.DAL.Storage.UpdateSectionHold
{
    public interface IUpdateSectionHoldStorage
    {
        Task UpdateStatusAsync(Guid id, ESeatSectionHoldStatus status, CancellationToken ct);
    }
}
