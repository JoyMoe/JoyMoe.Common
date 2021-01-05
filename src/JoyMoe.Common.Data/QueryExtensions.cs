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

        public static string Escape(this string name)
        {
            return $"\"{name}\"";
        }

        public static string PrepareSql(this string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return sql;

            var tokens = new List<string>();
            foreach (var token in sql.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                if (token[0] == '@' && char.IsLetterOrDigit(token, 1))
                {
                    tokens.Add(Escape(token[1..]));
                    continue;
                }

                tokens.Add(token);
            }

            return string.Join(' ', tokens);
        }
    }
}
