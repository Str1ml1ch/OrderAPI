using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Core.Enums;
using OrderAPI.Core.Models;

namespace OrderAPI.DAL.Storage.GetSectionHolds
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
