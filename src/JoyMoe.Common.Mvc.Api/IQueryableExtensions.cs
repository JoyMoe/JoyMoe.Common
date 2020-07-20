using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using JoyMoe.Common.Mvc.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    public static class PaginationResponseIQueryableExtensions
    {
        public async static Task<PaginationResponse<T>> ToPaginationResponseAsync<T, TEntity>(
            this IQueryable<IDataEntity> query,
            PaginationRequest request,
            Func<TEntity, int, T> expression)
            where T : IIdentifier
            where TEntity : IIdentifier
        {
            if (!(query is IQueryable<TEntity>))
            {
                throw new NotSupportedException();
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Before.HasValue)
            {
                query = query.Where(a => a.Id < request.Before);
            }

            var entities = await query.OrderByDescending(a => a.Id)
                .Take(request.Size)
                .ToListAsync()
                .ConfigureAwait(false);

            var data = entities.Cast<TEntity>().Select(expression).ToList();

            return new PaginationResponse<T>(data, request.Before);
        }

        public static PaginationResponse<T> ToPaginationResponse<T, TEntity>(
            this IQueryable<IDataEntity> query,
            PaginationRequest request,
            Func<TEntity, int, T> expression)
            where T : IIdentifier
            where TEntity : IIdentifier
        {
            return query.ToPaginationResponseAsync<T, TEntity>(request, expression)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
