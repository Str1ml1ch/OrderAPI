using System.Linq.Expressions;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.SeatHolds
{
    public sealed class SeatHoldByOrderIdSpecification : ISpecification<SeatHold>
    {
        private readonly Guid _orderId;
        public SeatHoldByOrderIdSpecification(Guid orderId) => _orderId = orderId;
        public Expression<Func<SeatHold, bool>> ToExpression() => sh => sh.OderId == _orderId;
    }
}
