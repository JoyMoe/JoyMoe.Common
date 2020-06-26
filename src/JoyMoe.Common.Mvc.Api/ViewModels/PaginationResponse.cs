using System;
using System.Linq;
using System.Linq.Expressions;
using JoyMoe.Common.EntityFrameworkCore.Model;

namespace JoyMoe.Common.Mvc.Api.ViewModels
{
    public class PaginationResponse<T> where T : IIdentifier
    {
        public long? Before { get; set; }

        public int Size { get; set; }

        public long? Last { get; set; }

        public T[] Data { get; set; }

        public static PaginationResponse<T> Create(T[] data, long? before = null)
        {
            return new PaginationResponse<T>
            {
                Before = before,
                Size = data.Length,
                Last = data.LastOrDefault()?.Id,
                Data = data
            };
        }

        public static PaginationResponse<T> Create<TEntity>(
            IQueryable<TEntity> query,
            PaginationRequest request,
            Expression<Func<TEntity, int, T>> expression)
            where TEntity : IIdentifier
        {
            if (request.Before.HasValue)
            {
                query = query.Where(a => a.Id < request.Before);
            }

            query = query.OrderByDescending(a => a.Id);

            var data = query
                .Take(request.Size)
                .Select(expression)
                .ToArray();

            return Create(data, request.Before);
        }
    }
}
