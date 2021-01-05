using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public override IAsyncEnumerable<TEntity> ListAsync(string? predicate, List<object>? values, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);

            return BuildQuery(Context, predicate, values ?? new List<object>())
                .AsAsyncEnumerable();
        }

        public override async ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            string? predicate = null,
            List<object>? values = null,
            bool everything = false,
            CancellationToken ct = default)
        {
            values ??= new List<object>();

            var key = selector.GetColumnName();

            if (before != null)
            {
                predicate = string.IsNullOrEmpty(predicate)
                    ? $"@{key} < {{{values.Count}}}"
                    : $"( {predicate} ) AND @{key} < {{{values.Count}}}";
                values.Add(before);
            }

            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values)
                .OrderByDescending(selector)
                .Take(size)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public override async ValueTask<TEntity?> FirstOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values ?? new List<object>())
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public override async ValueTask<TEntity?> SingleOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values ?? new List<object>())
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public override async ValueTask<bool> AnyAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values ?? new List<object>())
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public override async ValueTask<long> CountAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildQuery(Context, predicate, values ?? new List<object>())
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

        public override async ValueTask<int> CommitAsync(CancellationToken ct = default)
        {
            return await Context.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        private static IQueryable<TEntity> BuildQuery(TContext context, string? predicate, List<object> values)
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

            return context.Set<TEntity>().FromSqlRaw(sql, values.ToArray());
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
