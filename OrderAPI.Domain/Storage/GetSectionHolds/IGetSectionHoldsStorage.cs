using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.Storage.GetSectionHolds
{
    public interface IGetSectionHoldsStorage
    {
        Task<ResultModel<List<SectionHoldModel>>> GetAsync(
            int page,
            int pageSize,
            Guid? orderId,
            Guid? sectionId,
            ESeatSectionHoldStatus? status,
            CancellationToken ct);
    }
}
