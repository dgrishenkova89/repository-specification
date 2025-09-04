using RepositoryPattern.Models;
using System.Linq.Expressions;

namespace RepositoryPattern.Specifications
{
    public class TrueSpecification<T> : Specification<T>
        where T : BaseEntity
    {
        public override Expression<Func<T, bool>> Expression => True();
    }
}
