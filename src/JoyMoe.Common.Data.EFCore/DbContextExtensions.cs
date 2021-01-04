using System;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.Data.EFCore
{
    internal static class DbContextExtensions
    {
        public static string GetTableName<TEntity>(this DbContext context) where TEntity : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var type = context.Model.FindEntityType(typeof(TEntity));

            var schema = type.GetSchema();
            var table = type.GetTableName();

            return string.IsNullOrWhiteSpace(schema)
                ? $"\"{table}\""
                : $"\"{schema}\".\"{table}\"";
        }
    }
}
