using System.Linq.Expressions;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.SectionHolds
{
    public sealed class SectionHoldByOrderIdSpecification : ISpecification<SectionHold>
    {
        private readonly Guid _orderId;
        public SectionHoldByOrderIdSpecification(Guid orderId) => _orderId = orderId;
        public Expression<Func<SectionHold, bool>> ToExpression() => sh => sh.OderId == _orderId;
    }
}
