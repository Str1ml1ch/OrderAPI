using System.Linq.Expressions;

namespace OrderAPI.DAL.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
    }
}
