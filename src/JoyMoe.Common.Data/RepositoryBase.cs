using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public virtual ValueTask<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default) where TKey : struct
        {
            return SingleOrDefaultAsync($"@{selector.GetColumnName()} = {{0}}", new List<object> { id }, true, ct);
        }

        public virtual IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids) where TKey : struct
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var i = 0;
            var sb = new StringBuilder();
            var keys = new List<object>();
            foreach (var id in ids)
            {
                sb.Append($"{{{i}}}");
                sb.Append(" , ");

                keys.Add(id);

                i++;
            }

            var list = sb.ToString();

            return ListAsync($"@{selector.GetColumnName()} IN ( {list[..^3]} )", keys, true);
        }

        public abstract IAsyncEnumerable<TEntity> ListAsync(string? predicate, List<object>? values, bool everything = false);

        public abstract ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            string? predicate = null,
            List<object>? values = null,
            bool everything = false,
            CancellationToken ct = default)
            where TKey : struct, IComparable;

        public abstract ValueTask<TEntity?> FirstOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);

        public abstract ValueTask<TEntity?> SingleOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);

        public abstract ValueTask<bool> AnyAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);

        public abstract ValueTask<long> CountAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);

        public abstract Task AddAsync(TEntity entity, CancellationToken ct = default);

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                await AddAsync(entity, ct).ConfigureAwait(false);
            }
        }

        public abstract Task UpdateAsync(TEntity entity, CancellationToken ct = default);

        public abstract Task RemoveAsync(TEntity entity, CancellationToken ct = default);


        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                await RemoveAsync(entity, ct).ConfigureAwait(false);
            }
        }

        public abstract ValueTask<int> CommitAsync(CancellationToken ct = default);
    }
}
