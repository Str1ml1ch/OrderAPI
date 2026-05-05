using Homework.Ticketing.System.Shared.Models;
using OrderAPI.Domain.Enums;
using OrderAPI.Domain.Models;

namespace OrderAPI.Domain.Storage.GetSeatHolds
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
