using System;
using System.Linq.Expressions;

namespace JoyMoe.Common.Data;

public static class ExpressionExtensions
{
    public static MemberExpression GetColumn<TEntity, TKey>(this Expression<Func<TEntity, TKey>> selector)
        where TEntity : class {
        if (selector.Body is not MemberExpression key)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return key;
    }

    public static Expression<Func<TEntity, bool>>? And<TEntity>(
        this Expression<Func<TEntity, bool>>? left,
        Expression<Func<TEntity, bool>>?      right) {
        return CombinePredicates(left, right, ExpressionType.AndAlso);
    }

    public static Expression<Func<TEntity, bool>>? Or<TEntity>(
        this Expression<Func<TEntity, bool>>? left,
        Expression<Func<TEntity, bool>>?      right) {
        return CombinePredicates(left, right, ExpressionType.OrElse);
    }

    private static Expression<Func<T, bool>>? CombinePredicates<T>(
        this Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>>?      right,
        ExpressionType                  type) {
        if (left == null) return right;
        if (right == null) return left;

        var parameter = Expression.Parameter(typeof(T));

        var l = Replacer.Replace(left, parameter);
        var r = Replacer.Replace(right, parameter);

        var body = Expression.MakeBinary(type, l!, r!);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    internal class Replacer : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public static Expression? Replace(LambdaExpression? expression, ParameterExpression parameter) {
            if (expression == null) return null;

            var visitor = new Replacer(expression.Parameters[0], parameter);
            return visitor.Visit(expression.Body);
        }

        private Replacer(Expression oldValue, Expression newValue) {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression node) {
            if (node == _oldValue)
            {
                return _newValue;
            }

            return base.Visit(node);
        }
    }
}
