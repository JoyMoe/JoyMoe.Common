using System.Collections.Generic;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    public static class PaginationResponseIQueryableExtensions
    {
        public static async Task<IEnumerable<TEntity>> PaginateAsync<TEntity>(
            this IQueryable<TEntity> query,
            long? before = null, int size = 10)
            where TEntity : IDataEntity
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (before != null)
            {
                query = query.Where(a => a.Id < before);
            }

            return await query.OrderByDescending(a => a.Id)
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public static IEnumerable<TEntity> Paginate<TEntity>(
            this IQueryable<TEntity> query,
            long? before = null, int size = 10)
            where TEntity : IDataEntity
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (before != null)
            {
                query = query.Where(a => a.Id < before);
            }

            return query.OrderByDescending(a => a.Id)
                .Take(size)
                .ToList();
        }
    }
}

