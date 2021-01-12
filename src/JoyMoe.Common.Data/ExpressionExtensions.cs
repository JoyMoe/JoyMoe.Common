using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JoyMoe.Common.Data
{
    public static class ExpressionExtensions
    {
        public static MemberExpression GetColumn<TEntity, TKey>(this Expression<Func<TEntity, TKey>> selector) where TEntity : class
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (selector.Body is not MemberExpression key)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return key;
        }

        public static Expression<Func<TEntity, bool>>? And<TEntity>(
            this Expression<Func<TEntity, bool>>? left,
            Expression<Func<TEntity, bool>>? right)
        {
            return CombineLambdas(left, right, ExpressionType.AndAlso);
        }

        public static Expression<Func<TEntity, bool>>? Or<TEntity>(
            this Expression<Func<TEntity, bool>>? left,
            Expression<Func<TEntity, bool>>? right)
        {
            return CombineLambdas(left, right, ExpressionType.OrElse);
        }

        private static Expression<Func<T, bool>>? CombineLambdas<T>(
            this Expression<Func<T, bool>>? left,
            Expression<Func<T, bool>>? right,
            ExpressionType expressionType)
        {
            if (left == null) return right;
            if (right == null) return left;

            if (left.Body is ConstantExpression ce &&
                ce.Value.Equals(true))
            {
                return right;
            }

            var parameter = left.Parameters[0];

            ParameterVisitor visitor = new(right.Parameters[0], parameter);

            var rb = visitor.Visit(right.Body);

            if (rb == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            var body = Expression.MakeBinary(expressionType, left.Body, rb);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private class ParameterVisitor : ExpressionVisitor
        {
            private readonly KeyValuePair<Expression, Expression> _substitute;

            public ParameterVisitor(Expression parameter, Expression substitute )
            {
                _substitute = new KeyValuePair<Expression, Expression>(parameter, substitute);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _substitute.Key == node ? _substitute.Value : node;
            }
        }
    }
}
