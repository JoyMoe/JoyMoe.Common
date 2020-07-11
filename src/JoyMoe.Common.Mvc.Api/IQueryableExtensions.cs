using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using JoyMoe.Common.Mvc.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace System.Linq
{
    public static class PaginationResponseIQueryableExtensions
    {
        public async static Task<PaginationResponse<T>> ToPaginationResponseAsync<T, TEntity>(
            this IQueryable<TEntity> query,
            PaginationRequest request,
            Expression<Func<TEntity, int, T>> expression)
            where T : IIdentifier
            where TEntity : IIdentifier
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Before.HasValue)
            {
                query = query.Where(a => a.Id < request.Before);
            }

            var data = await query.OrderByDescending(a => a.Id)
                .Take(request.Size)
                .Select(expression)
                .ToListAsync()
                .ConfigureAwait(false);

            return new PaginationResponse<T>(data, request.Before);
        }

        public static PaginationResponse<T> ToPaginationResponse<T, TEntity>(
            this IQueryable<TEntity> query,
            PaginationRequest request,
            Expression<Func<TEntity, int, T>> expression)
            where T : IIdentifier
            where TEntity : IIdentifier
        {
            return query.ToPaginationResponseAsync(request, expression)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
