using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using JoyMoe.Common.Mvc.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace System.Linq
{
    public static class PaginationResponseIQueryableExtensions
    {
        private static IQueryable<T> prepare<T, TEntity>(
            this IQueryable<TEntity> query,
            PaginationRequest request,
            Expression<Func<TEntity, int, T>> expression)
            where T : IIdentifier
            where TEntity : IIdentifier
        {
            if (request.Before.HasValue)
            {
                query = query.Where(a => a.Id < request.Before);
            }

            query = query.OrderByDescending(a => a.Id);

            return query
                .Take(request.Size)
                .Select(expression);
        }

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

            var data = await prepare(query, request, expression)
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
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var data = prepare(query, request, expression)
                .ToList();

            return new PaginationResponse<T>(data, request.Before);
        }
    }
}
