using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class EntityTypeExtensions
    {
        public static void AddQueryFilter(this IMutableEntityType entityType, LambdaExpression expression)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var parameter = Expression.Parameter(entityType.ClrType);
            var filter = ReplacingExpressionVisitor.Replace(expression.Parameters.Single(), parameter, expression.Body);

            var queryFilter = entityType.GetQueryFilter();
            if (queryFilter != null)
            {
                var currentFilter = ReplacingExpressionVisitor.Replace(queryFilter.Parameters.Single(), parameter, queryFilter.Body);
                filter = Expression.AndAlso(currentFilter, filter);
            }

            var lambda = Expression.Lambda(filter, parameter);
            entityType.SetQueryFilter(lambda);
        }
    }
}
