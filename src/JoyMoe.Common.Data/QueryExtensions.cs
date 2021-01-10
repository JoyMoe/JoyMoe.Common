using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JoyMoe.Common.Data
{
    public static class QueryExtensions
    {
        public static string GetColumnName<TEntity, TKey>(this Expression<Func<TEntity, TKey>> selector) where TEntity : class
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (selector.Body is not MemberExpression key)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return $"{key.Member.Name}";
        }

        public static string Escape(this string token)
        {
            return $"\"{token}\"";
        }

        public static bool IsColumnName(this string token)
        {
            return token.Length >=2 && token[0] == '@' && char.IsLetterOrDigit(token, 1);
        }

        public static string EscapeColumnName(this string token)
        {
            return Escape(token[1..]);
        }
    }
}
