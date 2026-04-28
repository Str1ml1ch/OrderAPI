using System.Linq.Expressions;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.SectionHolds
{
    public sealed class SectionHoldBySectionIdSpecification : ISpecification<SectionHold>
    {
        private readonly Guid _sectionId;
        public SectionHoldBySectionIdSpecification(Guid sectionId) => _sectionId = sectionId;
        public Expression<Func<SectionHold, bool>> ToExpression() => sh => sh.SectionId == _sectionId;
    }
}
