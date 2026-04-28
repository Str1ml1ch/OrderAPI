using System.Linq.Expressions;
using OrderAPI.Core.Enums;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.SectionHolds
{
    public sealed class SectionHoldByStatusSpecification : ISpecification<SectionHold>
    {
        private readonly ESeatSectionHoldStatus _status;
        public SectionHoldByStatusSpecification(ESeatSectionHoldStatus status) => _status = status;
        public Expression<Func<SectionHold, bool>> ToExpression() => sh => sh.SectionHoldStatus == _status;
    }
}
