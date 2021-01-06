using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public virtual Task<TEntity?> FindAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey id,
            CancellationToken ct = default)
            where TKey : struct
        {
            return SingleOrDefaultAsync(true, $"@{selector.GetColumnName()} = {{0}}", ct, id);
        }

        public virtual async IAsyncEnumerable<TEntity> FindAllAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            IEnumerable<TKey> ids,
            [EnumeratorCancellation] CancellationToken ct = default)
            where TKey : struct
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

            sb.Remove(sb.Length - 3, 3);

            var list = sb.ToString();
            var enumerable = ListAsync(true, $"@{selector.GetColumnName()} IN ( {list} )", values: keys.ToArray(), ct: ct);
            await foreach (var entity in enumerable.WithCancellation(ct))
            {
                yield return entity;
            }
        }

        public abstract IAsyncEnumerable<TEntity> ListAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values);

        public abstract Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
            where TKey : struct, IComparable;

        public abstract Task<TEntity?> FirstOrDefaultAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values);

        public abstract Task<TEntity?> SingleOrDefaultAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values);

        public abstract Task<bool> AnyAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values);

        public abstract Task<long> CountAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values);

        public virtual Task OnBeforeAddAsync(TEntity entity, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            if (entity is ITimestamp stamp)
            {
                stamp.CreatedAt = now;
                stamp.UpdatedAt = now;
            }

            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = null;
            }

            return Task.CompletedTask;
        }

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

        public virtual Task OnBeforeUpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity is ITimestamp stamp)
            {
                stamp.UpdatedAt = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }

        public abstract Task UpdateAsync(TEntity entity, CancellationToken ct = default);

        public virtual Task<bool> OnBeforeRemoveAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = DateTime.UtcNow;

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

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

        public abstract Task<int> CommitAsync(CancellationToken ct = default);
    }
}
