using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public bool IgnoreQueryFilters { get; set; }

        public Task<TEntity?> FindAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey id,
            CancellationToken ct = default)
            where TKey : struct
        {
            var key = selector.GetColumn();

            var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}");

            var property = Expression.Property(parameter, key.Member.Name);

            var equipment = Expression.Equal(property, Expression.Constant(id));

            var predicate = Expression.Lambda<Func<TEntity, bool>>(equipment, parameter);

            IgnoreQueryFilters = true;
            return SingleOrDefaultAsync(predicate, ct);
        }

        public IAsyncEnumerable<TEntity> FindAllAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            IEnumerable<TKey> ids,
            CancellationToken ct = default)
            where TKey : struct
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var key = selector.GetColumn();

            var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}");

            var property = Expression.Property(parameter, key.Member.Name);

            var contains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var body = Expression.Call(contains, Expression.Constant(ids), property);
            var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

            IgnoreQueryFilters = true;
            return ListAsync(predicate, selector, null, ct);
        }

        public virtual IAsyncEnumerable<TEntity> ListAsync(
            Expression<Func<TEntity, bool>>? predicate,
            CancellationToken ct = default)
        {
            return ListAsync<int>(predicate, null, null, ct);
        }

        public abstract IAsyncEnumerable<TEntity> ListAsync<TKey>(
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TKey>>? ordering,
            int? limitation,
            CancellationToken ct = default)
            where TKey : struct;

        public virtual async Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken ct = default)
            where TKey : struct, IComparable
        {
            if (before != null)
            {
                var key = selector.GetColumn();

                var parameter = predicate == null
                    ? Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}")
                    : predicate.Parameters[0];

                var property = Expression.Property(parameter, key.Member.Name);
                var less = Expression.LessThan(property, Expression.Constant(before));

                var binary = predicate != null
                    ? Expression.And(predicate.Body, less)
                    : less;

                predicate = Expression.Lambda<Func<TEntity, bool>>(binary, parameter);
            }

            return await ListAsync(predicate, selector, size, ct).ToListAsync(ct).ConfigureAwait(false);
        }

        public abstract Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

        public abstract Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

        public abstract Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

        public abstract Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

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

        public virtual Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var tasks = entities.Select(e => AddAsync(e, ct)).ToArray();

            return Task.WhenAny(tasks);
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

        public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var tasks = entities.Select(e => RemoveAsync(e, ct)).ToArray();

            return Task.WhenAny(tasks);
        }

        public abstract Task<int> CommitAsync(CancellationToken ct = default);

        protected Expression<Func<TEntity, bool>>? FilteringQuery(Expression<Func<TEntity, bool>>? predicate)
        {
            if (IgnoreQueryFilters)
            {
                IgnoreQueryFilters = false;

                return predicate;
            }

            if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return predicate;
            }

            var parameter = predicate == null
                ? Expression.Parameter(typeof(ISoftDelete), $"__sd_{DateTime.Now.ToFileTime()}")
                : predicate.Parameters[0];

            var property = Expression.Property(parameter, nameof(ISoftDelete.DeletedAt));
            var equipment = Expression.Equal(property, Expression.Constant(null));

            var binary = predicate != null
                ? Expression.And(predicate.Body, equipment)
                : equipment;

            return Expression.Lambda<Func<TEntity, bool>>(binary, parameter);
        }
    }
}
