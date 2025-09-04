using RepositoryPattern.Models;

namespace RepositoryPattern.Abstractions
{
    public interface IDatabaseContextRepository<T> : IDatabaseContextRepository
        where T : BaseEntity
    {
    }
}
