using Microsoft.EntityFrameworkCore;
using RepositoryPattern.Enums;
using RepositoryPattern.Models;
using RepositoryPattern.Specifications;
using System.Linq.Expressions;

namespace RepositoryPattern.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity?> IncludeAll<TEntity>(this IQueryable<TEntity?> query, IEnumerable<string>? includedProperties = null)
            where TEntity : BaseEntity
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var includedPropertiesArray = includedProperties?.ToArray();

            if (includedPropertiesArray == null || !includedPropertiesArray.Any())
            {
                return query;
            }

            foreach (var property in includedPropertiesArray)
            {
                query = ((IQueryable<TEntity>)query).Include(property);
            }

            return query;
        }

        public static IQueryable<TEntity?> GetFilteredQueryWithSorting<TEntity>(
            this IQueryable<TEntity?> query,
            Specification<TEntity> specification,
            IEnumerable<string> includedProperties,
            bool noTracking,
            bool asSplitQuery,
            int skip = default,
            int take = default,
            IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
            SortingOrder? sortingOrder = null)
            where TEntity : BaseEntity
        {
            query = query.GetFilteredQuery(specification, includedProperties, noTracking, asSplitQuery);

            if (sortingExpressions != null)
            {
                query = query.ApplySorting(sortingExpressions, sortingOrder);
            }

            if (skip != default)
            {
                query = query.Skip(skip);
            }

            if (take != default)
            {
                query = query.Take(take);
            }

            return query;
        }

        public static IQueryable<TProjection?> GetFilteredQueryWithSorting<TEntity, TProjection>(
            this IQueryable<TEntity?> query,
            Specification<TEntity> specification,
            Expression<Func<TEntity?, TProjection>> projectExpression,
            IEnumerable<string> includedProperties,
            bool noTracking,
            bool asSplitQuery,
            int skip = default,
            int take = default,
            IEnumerable<Expression<Func<TEntity?, object>>>? sortingExpressions = null,
            SortingOrder? sortingOrder = null)
            where TEntity : BaseEntity
        {
            query = query.GetFilteredQueryWithSorting(
                specification, includedProperties, noTracking, asSplitQuery, skip, take, sortingExpressions, sortingOrder);

            return query.Select(projectExpression);
        }

        public static IQueryable<TProjection?> GetDistinctFilteredQueryWithSorting<TEntity, TProjection>(
            this IQueryable<TEntity?> query,
            Specification<TEntity> specification,
            Expression<Func<TEntity?, TProjection>> projectExpression,
            IEnumerable<string> includedProperties,
            bool noTracking,
            bool asSplitQuery,
            int skip = default,
            int take = default,
            SortingOrder? sortingOrder = null)
            where TEntity : BaseEntity
        {
            query = query.GetFilteredQuery(specification, includedProperties, noTracking, asSplitQuery);

            var projectionQuery = ApplySortingAndDistinct();

            if (skip != default)
            {
                projectionQuery = projectionQuery.Skip(skip);
            }

            if (take != default)
            {
                projectionQuery = projectionQuery.Take(take);
            }

            return projectionQuery;

            IQueryable<TProjection> ApplySortingAndDistinct()
            {
                var selectedProjectionQuery = query
                    .Select(projectExpression)
                    .Distinct();

                sortingOrder ??= SortingOrder.Asc;

                selectedProjectionQuery = sortingOrder == SortingOrder.Asc
                    ? selectedProjectionQuery.OrderBy(projection => projection)
                    : selectedProjectionQuery.OrderByDescending(projection => projection);

                return selectedProjectionQuery;
            }
        }

        public static IQueryable<TProjection?> GetDistinctFilteredQueryWithSorting<TEntity, TProjection, TGroupKey>(
              this IQueryable<TEntity?> query,
              Specification<TEntity> specification,
              Expression<Func<TEntity?, TGroupKey>> groupByExpression,
              Expression<Func<TGroupKey, TProjection>> projectExpression,
              Expression<Func<TEntity?, object>> sortingExpression,
              int skip,
              int take,
              IEnumerable<string> includedProperties,
              bool noTracking,
              bool asSplitQuery)
            where TEntity : BaseEntity
        {
            query = query.GetFilteredQuery(specification, includedProperties, noTracking, asSplitQuery);

            var projectionQuery = query
                .OrderBy(sortingExpression)
                .GroupBy(groupByExpression)
                .Select(x => x.Key)
                .Select(projectExpression);

            if (skip != default)
            {
                projectionQuery = projectionQuery.Skip(skip);
            }

            if (take != default)
            {
                projectionQuery = projectionQuery.Take(take);
            }

            return projectionQuery;
        }

        public static IQueryable<TProjection> GetDistinctFilteredQueryWithSortingOptimized<TEntity, TProjection, TGroupKey>(
              this IQueryable<TEntity?> query,
              Specification<TEntity> specification,
              Expression<Func<TEntity?, TGroupKey>> groupByExpression,
              Expression<Func<TGroupKey, TProjection>> projectExpression,
              Expression<Func<TGroupKey, object>> sortingExpression,
              int skip,
              int take,
              IEnumerable<string> includedProperties,
              bool noTracking,
              bool asSplitQuery)
            where TEntity : BaseEntity
        {
            query = query.GetFilteredQuery(specification, includedProperties, noTracking, asSplitQuery);

            var distinctQuery = query
                .GroupBy(groupByExpression)
                .Select(x => x.Key);

            if (sortingExpression != null)
            {
                distinctQuery = distinctQuery.OrderBy(sortingExpression);
            }

            if (skip != default)
            {
                distinctQuery = distinctQuery.Skip(skip);
            }

            if (take != default)
            {
                distinctQuery = distinctQuery.Take(take);
            }

            var projectionQuery = distinctQuery.Select(projectExpression);

            return projectionQuery;
        }

        public static IQueryable<TProjection?> GetGroupByFilteredQueryWithSorting<TEntity, TProjection, TGroupKey>(
              this IQueryable<TEntity?> query,
              Specification<TEntity> specification,
              Expression<Func<TEntity?, TGroupKey>> groupByExpression,
              Func<IQueryable<IGrouping<TGroupKey, TEntity?>>, IQueryable<TGroupKey>> postGroupByFilter,
              Expression<Func<TGroupKey, TProjection>> projectExpression,
              Expression<Func<TProjection?, object>>? sortingExpression,
              int skip,
              int take,
              IEnumerable<string> includedProperties,
              bool noTracking,
              bool asSplitQuery)
            where TEntity : BaseEntity
        {
            query = query.GetFilteredQuery(specification, includedProperties, noTracking, asSplitQuery);

            var groupedQuery = query.GroupBy(groupByExpression);

            var postGroupByQuery = postGroupByFilter(groupedQuery);

            if (skip != default)
            {
                postGroupByQuery = postGroupByQuery.Skip(skip);
            }

            if (take != default)
            {
                postGroupByQuery = postGroupByQuery.Take(take);
            }

            var finalProjectionQuery = postGroupByQuery
                .Select(projectExpression);

            if (sortingExpression != null)
            {
                finalProjectionQuery = finalProjectionQuery.OrderBy(sortingExpression!);
            }

            return finalProjectionQuery;
        }

        public static IQueryable<TEntity?> GetFilteredQuery<TEntity>(
            this IQueryable<TEntity?> query,
            Specification<TEntity> specification,
            IEnumerable<string> includedProperties,
            bool noTracking,
            bool asSplitQuery)
            where TEntity : BaseEntity
        {
            query = query.IncludeAll(includedProperties);

            if (noTracking)
            {
                query = ((IQueryable<TEntity>)query).AsNoTracking();
            }

            if (asSplitQuery)
            {
                query = ((IQueryable<TEntity>)query).AsSplitQuery();
            }

            query = query.DefaultIfEmpty();

            var expression = specification.Expression as Expression<Func<TEntity?, bool>>;
            query = query.Where(expression);

            return query;
        }

        private static IQueryable<TEntity?> ApplySorting<TEntity>(
            this IQueryable<TEntity?> query,
            IEnumerable<Expression<Func<TEntity?, object>>> sortingExpressions,
            SortingOrder? sortingOrder)
            where TEntity : BaseEntity
        {
            sortingOrder ??= SortingOrder.Asc;

            var i = 0;
            foreach (var sortingExpression in sortingExpressions)
            {
                if (i == 0)
                {
                    query = sortingOrder == SortingOrder.Asc
                        ? query.OrderBy(sortingExpression)
                        : query.OrderByDescending(sortingExpression);
                }
                else
                {
                    var orderedQuery = (IOrderedQueryable<TEntity?>)query;

                    query = sortingOrder == SortingOrder.Asc
                        ? orderedQuery.ThenBy(sortingExpression)
                        : orderedQuery.ThenByDescending(sortingExpression);
                }

                i++;
            }

            return query;
        }
    }

}
