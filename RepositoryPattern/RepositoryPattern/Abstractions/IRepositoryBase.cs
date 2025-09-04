using Microsoft.EntityFrameworkCore;
using RepositoryPattern.Enums;
using RepositoryPattern.Models;
using RepositoryPattern.Specifications;
using System.Linq.Expressions;

namespace RepositoryPattern.Abstractions
{
    public interface IRepositoryBase<out TDbContext>
            where TDbContext : DbContext
    {
        TDbContext Context { get; }

        Task<long> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
            where TEntity : BaseEntity;

        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
            where TEntity : BaseEntity;

        Task<bool> AnyAsync<TEntity>(
                Specification<TEntity> specification,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                bool asSplitQuery = false)
            where TEntity : BaseEntity;

        Task<int> CountAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken, bool asSplitQuery = false, bool useDistinct = false)
            where TEntity : BaseEntity;

        Task<int> CountAsync<TEntity, TProjection>(
                        Specification<TEntity> specification,
                        Expression<Func<TEntity?, TProjection>> projectExpression,
                        CancellationToken cancellationToken,
                        bool asSplitQuery = false,
                        bool useDistinct = false)
                    where TEntity : BaseEntity;

        Task DeleteAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken)
            where TEntity : BaseEntity;

        Task DeleteRangeAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken)
            where TEntity : BaseEntity;

        Task<TEntity?> FirstOrDefaultAsync<TEntity>(
                Specification<TEntity> specification,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                bool asSplitQuery = false,
                IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
                SortingOrder? sortingOrder = null)
            where TEntity : BaseEntity;

        Task<TProjection?> FirstOrDefaultAsync<TEntity, TProjection>(
                Specification<TEntity> specification,
                Expression<Func<TEntity?, TProjection>> projectExpression,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                bool asSplitQuery = true,
                IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
                SortingOrder? sortingOrder = null)
            where TEntity : BaseEntity;

        Task<TEntity[]> GetArrayAsync<TEntity>(
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
            where TEntity : BaseEntity;

        Task<TProjection[]> GetArrayAsync<TEntity, TProjection>(
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
            where TEntity : BaseEntity;

        Task<TProjection[]> GetDistinctItemsArrayAsync<TEntity, TProjection>(
                Specification<TEntity> specification,
                Expression<Func<TEntity?, TProjection>> projectExpression,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null,
                bool noTracking = true,
                int skip = default,
                int take = default,
                SortingOrder? sortingOrder = null,
                bool asSplitQuery = false)
            where TEntity : BaseEntity;

        Task<TProjection[]> GetDistinctItemsArrayAsync<TEntity, TProjection, TGroupKey>(
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
            where TEntity : BaseEntity;

        Task<TProjection[]> GetDistinctItemsArrayWithSortingOptimizedAsync<TEntity, TProjection, TGroupKey>(
              Specification<TEntity> specification,
              Expression<Func<TEntity?, TGroupKey>> groupByExpression,
              Expression<Func<TGroupKey, TProjection>> projectExpression,
              Expression<Func<TGroupKey, object>> sortingExpression,
              CancellationToken token,
              int skip = default,
              int take = default,
              IEnumerable<string>? includedProperties = null,
              bool noTracking = true,
              bool asSplitQuery = false)
            where TEntity : BaseEntity;

        Task<TProjection[]> GetDistinctItemsArrayAsync<TEntity, TProjection, TGroupKey>(
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
          where TEntity : BaseEntity;

        Task UpdateAsync<TEntity>(
                Specification<TEntity> specification,
                Action<TEntity> updateAction,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null)
            where TEntity : BaseEntity;

        Task UpdateRangeAsync<TEntity>(
                Specification<TEntity> specification,
                Action<IEnumerable<TEntity>> updateAction,
                CancellationToken cancellationToken,
                IEnumerable<string>? includedProperties = null)
            where TEntity : BaseEntity;
    }
}
