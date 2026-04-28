using System.Linq.Expressions;
using OrderAPI.Core.Enums;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.Orders
{
    public sealed class OrderByCustomerIdSpecification : ISpecification<Order>
    {
        private readonly Guid _customerId;
        public OrderByCustomerIdSpecification(Guid customerId) => _customerId = customerId;
        public Expression<Func<Order, bool>> ToExpression() => o => o.CustomerId == _customerId;
    }
}
