using System;
using System.Linq.Expressions;

namespace JoyMoe.Common.Data
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression? right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                return left;
            }

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left.Body, right),
                left.Parameters
            );
        }
    }
}
