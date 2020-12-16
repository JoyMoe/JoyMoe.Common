using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.Data.EFCore
{
    public class EntityFrameworkCoreRepository<TEntity> : EntityFrameworkCoreRepository<DbContext, TEntity>
        where TEntity : class
    {
        public EntityFrameworkCoreRepository(DbContext context) : base(context)
        {
        }
    }

    public class EntityFrameworkCoreRepository<TContext, TEntity> : IRepository<TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        protected TContext Context { get; }

        public EntityFrameworkCoreRepository(TContext context)
        {
            Context = context;
        }

        public virtual async ValueTask<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default)
            where TKey : struct
        {
            if (selector?.Body is not MemberExpression key)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}");

            var property = Expression.Property(parameter, key.Member.Name);

            var equipment = Expression.Equal(property, Expression.Constant(key));

            var predicate = Expression.Lambda<Func<TEntity, bool>>(equipment, parameter);

            return await Context.Set<TEntity>().SingleOrDefaultAsync(predicate, cancellationToken: ct).ConfigureAwait(false);
        }

        public virtual IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids)
            where TKey : struct
        {
            if (selector?.Body is not MemberExpression key)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}");

            var property = Expression.Property(parameter, key.Member.Name);

            var contains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(int));

            var body = Expression.Call(contains, Expression.Constant(ids), property);
            var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

            return Context.Set<TEntity>().Where(predicate).AsAsyncEnumerable();
        }

        public virtual IAsyncEnumerable<TEntity> ListAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);

            return predicate == null
                ? Context.Set<TEntity>().AsAsyncEnumerable()
                : Context.Set<TEntity>().Where(predicate).AsAsyncEnumerable();
        }

        public virtual async ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            Expression<Func<TEntity, bool>>? predicate = null,
            bool everything = false,
            CancellationToken ct = default)
            where TKey : struct, IComparable
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (before != null)
            {
                if (selector.Body is not MemberExpression key)
                {
                    throw new ArgumentNullException(nameof(selector));
                }

                var parameter = predicate == null
                    ? Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}")
                    : predicate.Parameters[0];

                var property = Expression.Property(parameter, key.Member.Name);

                var less = Expression.LessThan(property, Expression.Constant(before));

                predicate = predicate == null
                    ? Expression.Lambda<Func<TEntity, bool>>(less, parameter)
                    : predicate.And(less);
            }

            predicate = FilteringQuery(predicate, everything);

            var query = predicate == null
                ? Context.Set<TEntity>()
                : Context.Set<TEntity>().Where(predicate);

            return await query
                .OrderByDescending(selector)
                .Take(size)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public virtual async ValueTask<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return predicate == null
                ? await Context.Set<TEntity>().SingleOrDefaultAsync(ct).ConfigureAwait(false)
                : await Context.Set<TEntity>().SingleOrDefaultAsync(predicate, ct).ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return predicate == null
                ? await Context.Set<TEntity>().AnyAsync(ct).ConfigureAwait(false)
                : await Context.Set<TEntity>().AnyAsync(predicate, ct).ConfigureAwait(false);
        }

        public virtual async ValueTask<int> CountAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return predicate == null
                ? await Context.Set<TEntity>().CountAsync(ct).ConfigureAwait(false)
                : await Context.Set<TEntity>().CountAsync(predicate, ct).ConfigureAwait(false);
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

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

            await Context.AddAsync(entity, ct).ConfigureAwait(false);
        }

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

        public virtual Task UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity is ITimestamp stamp)
            {
                stamp.UpdatedAt = DateTime.UtcNow;
            }

            Context.Entry(entity).State = EntityState.Detached;
            Context.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task RemoveAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = DateTime.UtcNow;
                return Task.CompletedTask;
            }

            Context.Remove(entity);
            return Task.CompletedTask;
        }

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

        public virtual async ValueTask<int> CommitAsync(CancellationToken ct = default)
        {
            return await Context.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        private static Expression<Func<TEntity, bool>>? FilteringQuery(Expression<Func<TEntity, bool>>? predicate, bool everything)
        {
            if (everything)
            {
                return predicate;
            }

            var parameter = predicate == null
                ? Expression.Parameter(typeof(ISoftDelete), $"__sd_{DateTime.Now.ToFileTime()}")
                : predicate.Parameters[0];

            var property = Expression.Property(parameter, nameof(ISoftDelete.DeletedAt));
            var equipment = Expression.Equal(property, Expression.Constant(null));

            return predicate == null
                ? Expression.Lambda<Func<TEntity, bool>>(equipment, parameter)
                : predicate.And(equipment);
        }
    }
}
