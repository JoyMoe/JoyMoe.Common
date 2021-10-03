using System;
using System.Linq.Expressions;

namespace JoyMoe.Common.Data;

public static class ExpressionExtensions
{
    public static MemberExpression GetColumn<TEntity, TKey>(this Expression<Func<TEntity, TKey>> selector)
        where TEntity : class
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
        Expression<Func<TEntity, bool>>?      right)
    {
        return CombinePredicates(left, right, ExpressionType.AndAlso);
    }

    public static Expression<Func<TEntity, bool>>? Or<TEntity>(
        this Expression<Func<TEntity, bool>>? left,
        Expression<Func<TEntity, bool>>?      right)
    {
        return CombinePredicates(left, right, ExpressionType.OrElse);
    }

    private static Expression<Func<T, bool>>? CombinePredicates<T>(
        this Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>>?      right,
        ExpressionType                  expressionType)
    {
        if (left == null) return right;
        if (right == null) return left;

        if (left.Body is ConstantExpression ce &&
            ce.Value.Equals(true))
        {
            return right;
        }

        var body = Expression.MakeBinary(expressionType, left.Body, right.Body);

        return Expression.Lambda<Func<T, bool>>(body, left.Parameters[0]);
    }
}
