using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Core.Enums;
using OrderAPI.Core.Models;

namespace OrderAPI.DAL.Storage.GetSeatHolds
{
    public interface IGetSeatHoldsStorage
    {
        Task<ResultModel<List<SeatHoldModel>>> GetAsync(
            int page,
            int pageSize,
            Guid? orderId,
            ESeatSectionHoldStatus? status,
            CancellationToken ct);
    }
}
