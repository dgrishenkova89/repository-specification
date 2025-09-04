using Microsoft.EntityFrameworkCore;
using RepositoryPattern.Abstractions;
using RepositoryPattern.Enums;
using RepositoryPattern.Extensions;
using RepositoryPattern.Models;
using RepositoryPattern.Specifications;
using System.Linq.Expressions;

namespace RepositoryPattern.Repositories
{
    public abstract class RepositoryBase<TDbContext> : IRepositoryBase<TDbContext>
        where TDbContext : DbContext
    {
        public TDbContext Context { get; }

        protected virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : BaseEntity
        {
            return Context.Set<TEntity>();
        }

        protected RepositoryBase(TDbContext context)
        {
            Context = context;
        }

        public virtual async Task<long> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
            where TEntity : BaseEntity
        {
            var entry = Context.Add(entity);

            await SaveChangesAsync(cancellationToken);

            return entry.Entity.Id;
        }

        public virtual Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
            where TEntity : BaseEntity
        {
            Context.AddRange(entities);

            return SaveChangesAsync(cancellationToken);
        }

        public Task<bool> AnyAsync<TEntity>(
                Specification<TEntity> specification,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                bool asSplitQuery = false)
            where TEntity : BaseEntity
        {
            var query = (IQueryable<TEntity>)Set<TEntity>()
                .IncludeAll(includedProperties);

            if (asSplitQuery)
            {
                query = query.AsSplitQuery();
            }

            return query.AnyAsync(specification.Expression, cancellationToken);
        }

        public Task<int> CountAsync<TEntity>(
                Specification<TEntity> specification,
                CancellationToken cancellationToken,
                bool asSplitQuery = false,
                bool useDistinct = false)
            where TEntity : BaseEntity
        {
            var query = Set<TEntity>().GetFilteredQuery(specification, Array.Empty<string>(), true, asSplitQuery);
            return query
                .CountAsync(cancellationToken);
        }

        public Task<int> CountAsync<TEntity, TProjection>(
                Specification<TEntity> specification,
                Expression<Func<TEntity?, TProjection>> projectExpression,
                CancellationToken cancellationToken,
                bool asSplitQuery = false,
                bool useDistinct = false)
            where TEntity : BaseEntity
        {
            var query = Set<TEntity>()
                .GetDistinctFilteredQueryWithSorting(specification, projectExpression, Array.Empty<string>(), true, asSplitQuery);
            return query.CountAsync(cancellationToken);
        }

        public async Task DeleteAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken)
            where TEntity : BaseEntity
        {
            var entity = await Set<TEntity>().FirstOrDefaultAsync(specification.Expression, cancellationToken) ??
                throw new InvalidOperationException(
                    $"Could not find entity {typeof(TEntity).Name} by specification {specification.GetType().Name}");

            Context.Remove(entity);

            await SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteRangeAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken)
            where TEntity : BaseEntity
        {
            object[] entities = await GetArrayAsync(specification, cancellationToken, noTracking: false);

            Context.RemoveRange(entities);

            await SaveChangesAsync(cancellationToken);
        }

        public Task<TEntity?> FirstOrDefaultAsync<TEntity>(
            Specification<TEntity> specification,
            CancellationToken cancellationToken,
            IEnumerable<string>? includedProperties = null,
            bool noTracking = true,
            bool asSplitQuery = false,
            IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
            SortingOrder? sortingOrder = null)
            where TEntity : BaseEntity
        {
            var query = Set<TEntity>().GetFilteredQueryWithSorting(
                specification, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery, sortingExpressions: sortingExpressions, sortingOrder: sortingOrder);

            return query.FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TProjection?> FirstOrDefaultAsync<TEntity, TProjection>(
            Specification<TEntity> specification,
            Expression<Func<TEntity?, TProjection>> projectExpression,
            CancellationToken cancellationToken,
            IEnumerable<string>? includedProperties = null,
            bool noTracking = true,
            bool asSplitQuery = true,
            IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
            SortingOrder? sortingOrder = null)
            where TEntity : BaseEntity
        {
            var query = Set<TEntity>().GetFilteredQueryWithSorting(
                specification, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery, sortingExpressions: sortingExpressions, sortingOrder: sortingOrder);

            return query.Select(projectExpression).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TProjection[]> GetDistinctItemsArrayAsync<TEntity, TProjection>(
                Specification<TEntity> specification,
                Expression<Func<TEntity?, TProjection>> projectExpression,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                int skip = default,
                int take = default,
                SortingOrder? sortingOrder = null,
                bool asSplitQuery = false)
            where TEntity : BaseEntity
        {
            var query = (IQueryable<TProjection>)Set<TEntity>().GetDistinctFilteredQueryWithSorting(
                specification, projectExpression, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery, skip, take, sortingOrder);

            return query.ToArrayAsync(cancellationToken);
        }

        public Task<TProjection[]> GetDistinctItemsArrayAsync<TEntity, TProjection, TGroupKey>(
              Specification<TEntity> specification,
              Expression<Func<TEntity?, TGroupKey>> groupByExpression,
              Expression<Func<TGroupKey, TProjection>> projectExpression,
              Expression<Func<TEntity?, object>> sortingExpression,
              CancellationToken token,
              int skip = default,
              int take = default,
              IEnumerable<string>? includedProperties = null,
              bool noTracking = true,
              bool asSplitQuery = false)
              where TEntity : BaseEntity
        {
            var query = (IQueryable<TProjection>)Set<TEntity>().GetDistinctFilteredQueryWithSorting(
                specification, groupByExpression, projectExpression, sortingExpression, skip, take, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery);

            return query.ToArrayAsync(token);
        }

        public Task<TProjection[]> GetDistinctItemsArrayWithSortingOptimizedAsync<TEntity, TProjection, TGroupKey>(
          Specification<TEntity> specification,
          Expression<Func<TEntity?, TGroupKey>> groupByExpression,
          Expression<Func<TGroupKey, TProjection>> projectExpression,
          Expression<Func<TGroupKey, object>>? sortingExpression,
          CancellationToken token,
          int skip = default,
          int take = default,
          IEnumerable<string>? includedProperties = null,
          bool noTracking = true,
          bool asSplitQuery = false)
          where TEntity : BaseEntity
        {
            var query = Set<TEntity>().GetDistinctFilteredQueryWithSortingOptimized(
                specification, groupByExpression, projectExpression, sortingExpression!, skip, take, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery);

            return query.ToArrayAsync(token);
        }

        public Task<TProjection[]> GetDistinctItemsArrayAsync<TEntity, TProjection, TGroupKey>(
              Specification<TEntity> specification,
              Expression<Func<TEntity?, TGroupKey>> groupByExpression,
              Func<IQueryable<IGrouping<TGroupKey, TEntity?>>, IQueryable<TGroupKey>> postGroupByFilter,
              Expression<Func<TGroupKey, TProjection>> projectExpression,
              CancellationToken token,
              int skip = default,
              int take = default,
              IEnumerable<string>? includedProperties = null,
              Expression<Func<TProjection?, object>>? sortingExpression = null,
              bool noTracking = true,
              bool asSplitQuery = false)
          where TEntity : BaseEntity
        {
            var query = (IQueryable<TProjection>)Set<TEntity>().GetGroupByFilteredQueryWithSorting(
                specification, groupByExpression, postGroupByFilter, projectExpression, sortingExpression, skip, take, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery);

            return query.ToArrayAsync(token);
        }

        public Task<TEntity[]> GetArrayAsync<TEntity>(
                Specification<TEntity> specification,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                int skip = default,
                int take = default,
                IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
                SortingOrder? sortingOrder = null,
                bool asSplitQuery = false,
                bool useDistinct = false)
            where TEntity : BaseEntity
        {
            var query = (IQueryable<TEntity>)Set<TEntity>().GetFilteredQueryWithSorting(
                specification, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery, skip, take, sortingExpressions, sortingOrder);

            return query.ToArrayAsync(cancellationToken);
        }

        public Task<TProjection[]> GetArrayAsync<TEntity, TProjection>(
                Specification<TEntity> specification,
                Expression<Func<TEntity?, TProjection>> projectExpression,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                int skip = default,
                int take = default,
                IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
                SortingOrder? sortingOrder = null,
                bool asSplitQuery = false,
                bool useDistinct = false)
            where TEntity : BaseEntity
        {
            var query = (IQueryable<TProjection>)Set<TEntity>().GetFilteredQueryWithSorting(
                specification, projectExpression, includedProperties ?? Array.Empty<string>(), noTracking, asSplitQuery, skip, take, sortingExpressions, sortingOrder);

            return query.ToArrayAsync(cancellationToken);
        }

        public async Task UpdateAsync<TEntity>(
            Specification<TEntity> specification,
            Action<TEntity> updateAction,
            CancellationToken cancellationToken,
            IEnumerable<string>? includedProperties = null)
            where TEntity : BaseEntity
        {
            var originalEntity = await FirstOrDefaultAsync(specification, cancellationToken, includedProperties, false)
                ?? throw new Exception($"Could not find entity {typeof(TEntity).Name} by specification {specification.GetType().Name}");

            updateAction(originalEntity);

            await SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRangeAsync<TEntity>(
            Specification<TEntity> specification,
            Action<IEnumerable<TEntity>> updateAction,
            CancellationToken cancellationToken,
            IEnumerable<string>? includedProperties = null)
            where TEntity : BaseEntity
        {
            var originalEntitiesList = await GetArrayAsync(specification, cancellationToken, includedProperties, false);

            updateAction(originalEntitiesList);

            await SaveChangesAsync(cancellationToken);
            }

        private Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            var modifiedAuditedEntities = Context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(entity => entity.State == EntityState.Modified)
                .Select(entity => entity.Entity)
                .ToList();

            modifiedAuditedEntities.ForEach(entity =>
            {
                entity.UpdatedWhen = now;
            });

            return Context.SaveChangesAsync(cancellationToken);
        }
    }
}
