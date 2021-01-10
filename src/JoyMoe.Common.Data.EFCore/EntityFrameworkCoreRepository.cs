using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public override IAsyncEnumerable<TEntity> ListAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return BuildQuery(Context, predicate, values).AsAsyncEnumerable();
        }

        public override async Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            string? predicate = null,
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

            predicate = FilteringQuery(predicate);

            return await BuildQuery(Context, predicate, values)
                .OrderByDescending(selector)
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public override async Task<TEntity?> FirstOrDefaultAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return await BuildQuery(Context, predicate, values)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public override async Task<TEntity?> SingleOrDefaultAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return await BuildQuery(Context, predicate, values)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public override async Task<bool> AnyAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return await BuildQuery(Context, predicate, values)
                .AnyAsync()
                .ConfigureAwait(false);
        }

        public override async Task<long> CountAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return await BuildQuery(Context, predicate, values)
                .LongCountAsync()
                .ConfigureAwait(false);
        }

        public override async Task AddAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await OnBeforeAddAsync(entity).ConfigureAwait(false);

            await Context.AddAsync(entity).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await OnBeforeUpdateAsync(entity).ConfigureAwait(false);

            Context.Entry(entity).State = EntityState.Detached;
            Context.Update(entity);
        }

        public override async Task RemoveAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (await OnBeforeRemoveAsync(entity).ConfigureAwait(false))
            {
                Context.Remove(entity);
                return;
            }

            Context.Entry(entity).State = EntityState.Detached;
            Context.Update(entity);
        }

        public override async Task<int> CommitAsync()
        {
            return await Context.SaveChangesAsync().ConfigureAwait(false);
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
                : $"SELECT * FROM {name} WHERE {PreparePredicate(predicate)}";

            return context.Set<TEntity>().FromSqlRaw(sql, values);
        }

        private static string PreparePredicate(string predicate)
        {
            if (string.IsNullOrWhiteSpace(predicate)) return predicate;

            var tokens = new List<string>();
            foreach (var token in predicate.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                if (token.IsColumnName())
                {
                    tokens.Add(token.EscapeColumnName());
                    continue;
                }

                tokens.Add(token);
            }

            return string.Join(' ', tokens);
        }
    }
}
