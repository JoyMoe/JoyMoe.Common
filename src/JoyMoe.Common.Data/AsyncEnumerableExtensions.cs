using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<TResult>? Map<TResult, T>(this IAsyncEnumerable<T>? source, Expression<Func<T, TResult>> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (source == null) yield break;

            var converter = mapper.Compile();
            await foreach (var item in source)
            {
                yield return converter(item);
            }
        }

        public static async Task<List<T>?> ToListAsync<T>(this IAsyncEnumerable<T>? source, CancellationToken ct = default)
        {
            if (source == null)
            {
                return null;
            }

            var result = new List<T>();

            await foreach (var item in source.WithCancellation(ct))
            {
                result.Add(item);
            }

            return result;
        }
    }
}
