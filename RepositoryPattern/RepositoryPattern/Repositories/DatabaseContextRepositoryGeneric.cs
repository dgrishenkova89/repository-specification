using RepositoryPattern.Abstractions;
using RepositoryPattern.Models;

namespace RepositoryPattern.Repositories
{
    public class DatabaseContextRepository<T> : RepositoryBase<DatabaseContext>, IDatabaseContextRepository<T>
        where T : BaseEntity
    {
        public DatabaseContextRepository(DatabaseContext context)
            : base(context)
        {
        }
    }
}
