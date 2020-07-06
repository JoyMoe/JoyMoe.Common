using System.Linq.Expressions;
using JoyMoe.Common.EntityFrameworkCore.Models;
using JoyMoe.Common.Mvc.Api.ViewModels;

namespace System.Linq
{
    public static class PaginationResponseIQueryableExtensions
    {
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

            if (request.Before.HasValue)
            {
                query = query.Where(a => a.Id < request.Before);
            }

            query = query.OrderByDescending(a => a.Id);

            var data = query
                .Take(request.Size)
                .Select(expression)
                .ToList();

            return new PaginationResponse<T>(data, request.Before);
        }
    }
}
