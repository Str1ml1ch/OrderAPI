using System.Linq.Expressions;
using OrderAPI.Core.Enums;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.SeatHolds
{
    public sealed class SeatHoldByStatusSpecification : ISpecification<SeatHold>
    {
        private readonly ESeatSectionHoldStatus _status;
        public SeatHoldByStatusSpecification(ESeatSectionHoldStatus status) => _status = status;
        public Expression<Func<SeatHold, bool>> ToExpression() => sh => sh.SeatHoldStatus == _status;
    }
}
