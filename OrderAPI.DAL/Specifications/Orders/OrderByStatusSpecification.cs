using System.Linq.Expressions;
using OrderAPI.Core.Enums;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL.Specifications.Orders
{
    public sealed class OrderByStatusSpecification : ISpecification<Order>
    {
        private readonly EOrderStatus _status;
        public OrderByStatusSpecification(EOrderStatus status) => _status = status;
        public Expression<Func<Order, bool>> ToExpression() => o => o.OrderStatus == _status;
    }
}
