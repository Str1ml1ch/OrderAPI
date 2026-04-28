using System.Linq.Expressions;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.Orders
{
    public sealed class OrderByEventIdSpecification : ISpecification<Order>
    {
        private readonly Guid _eventId;
        public OrderByEventIdSpecification(Guid eventId) => _eventId = eventId;
        public Expression<Func<Order, bool>> ToExpression() => o => o.EventId == _eventId;
    }
}
