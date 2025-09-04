using RepositoryPattern.Extensions;
using RepositoryPattern.Models;
using System.Linq.Expressions;

namespace RepositoryPattern.Specifications
{
    public class AndSpecification<T> : Specification<T>
        where T : BaseEntity
    {
        private readonly Specification<T> _spec1;
        private readonly Specification<T> _spec2;

        public AndSpecification(Specification<T> spec1, Specification<T> spec2)
        {
            _spec1 = spec1;
            _spec2 = spec2;
        }

        public override Expression<Func<T, bool>> Expression => _spec1.Expression.And(_spec2.Expression);
    }
}
