using System;
using System.Linq.Expressions;

namespace JoyMoe.Common.Data
{
    public static class QueryExtensions
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
    }
}
