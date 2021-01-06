using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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

    public class EntityFrameworkCoreRepository<TContext, TEntity> : RepositoryBase<TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        protected TContext Context { get; }

        public EntityFrameworkCoreRepository(TContext context)
        {
            Context = context;
        }

        public override async IAsyncEnumerable<TEntity> ListAsync(
            bool everything = false,
            string? predicate = null,
            [EnumeratorCancellation] CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            var enumerable = BuildQuery(Context, predicate, values).AsAsyncEnumerable();

            await foreach (var entity in enumerable.WithCancellation(ct))
            {
                yield return entity;
            }
        }

        public override async Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            var key = selector.GetColumnName();

            if (before != null)
            {
                predicate = string.IsNullOrEmpty(predicate)
                    ? $"@{key} < {{{values.Length}}}"
                    : $"( {predicate} ) AND @{key} < {{{values.Length}}}";

                Array.Resize(ref values, values.Length + 1);
                values[values.GetUpperBound(0)] = before;
            }

            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values)
                .OrderByDescending(selector)
                .Take(size)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public override async Task<TEntity?> FirstOrDefaultAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public override async Task<TEntity?> SingleOrDefaultAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values)
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public override async Task<bool> AnyAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values)
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public override async Task<long> CountAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values)
                .LongCountAsync(ct)
                .ConfigureAwait(false);
        }

        public override async Task AddAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await OnBeforeAddAsync(entity, ct).ConfigureAwait(false);

            await Context.AddAsync(entity, ct).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await OnBeforeUpdateAsync(entity, ct).ConfigureAwait(false);

            Context.Entry(entity).State = EntityState.Detached;
            Context.Update(entity);
        }

        public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (await OnBeforeRemoveAsync(entity, ct).ConfigureAwait(false))
            {
                Context.Remove(entity);
                return;
            }

            Context.Entry(entity).State = EntityState.Detached;
            Context.Update(entity);
        }

        public override async Task<int> CommitAsync(CancellationToken ct = default)
        {
            return await Context.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        private static IQueryable<TEntity> BuildQuery(TContext context, string? predicate, params object[] values)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (predicate == null)
            {
                return context.Set<TEntity>();
            }

            var type = context.Model.FindEntityType(typeof(TEntity));

            var schema = type.GetSchema();
            var table = type.GetTableName();

            var name = string.IsNullOrWhiteSpace(schema)
                ? table.Escape()
                : $"{schema.Escape()}.{table.Escape()}";

            string sql = string.IsNullOrWhiteSpace(predicate)
                ? $"SELECT * FROM {name}"
                : $"SELECT * FROM {name} WHERE {predicate.PrepareSql()}";

            return context.Set<TEntity>().FromSqlRaw(sql, values);
        }

        private static string? FilteringQuery(string? predicate, bool everything)
        {
            if (everything)
            {
                return predicate;
            }

            if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return predicate;
            }

            var filtering = $"@{nameof(ISoftDelete.DeletedAt)} IS NULL";

            return string.IsNullOrEmpty(predicate)
                ? filtering
                : $"( {predicate} ) AND {filtering}";
        }
    }
}
