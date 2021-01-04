using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

        public virtual ValueTask<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default)
            where TKey : struct
        {
            var key = GetColumnName(selector);

            return SingleOrDefaultAsync($"@{key} = {{0}}", new List<object> { id }, true, ct);
        }

        public virtual IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids)
            where TKey : struct
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var key = GetColumnName(selector);

            var i = 0;
            var sb = new StringBuilder();
            var keys = new List<object>();
            foreach (var id in ids)
            {
                sb.Append($"{{{i}}}");
                sb.Append(',');

                keys.Add(id);

                i++;
            }

            var list = sb.ToString().TrimEnd(',');

            return ListAsync($"@{key} IN ({list})", keys, true);
        }

        public virtual IAsyncEnumerable<TEntity> ListAsync(string? predicate, List<object>? values, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);

            return BuildSql(predicate, values ?? new List<object>())
                .AsAsyncEnumerable();
        }

        public virtual async ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            string? predicate = null,
            List<object>? values = null,
            bool everything = false,
            CancellationToken ct = default)
            where TKey : struct, IComparable
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            values ??= new List<object>();

            var key = GetColumnName(selector);

            if (before != null)
            {
                predicate = string.IsNullOrEmpty(predicate)
                    ? $"@{key} < {{{values.Count}}}"
                    : $"({predicate}) AND @{key} < {{{values.Count}}}";
                values.Add(before);
            }

            predicate = FilteringQuery(predicate, everything);

            return await BuildSql(predicate, values)
                .OrderByDescending(selector)
                .Take(size)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public virtual async ValueTask<TEntity?> FirstOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildSql(predicate, values ?? new List<object>())
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public virtual async ValueTask<TEntity?> SingleOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildSql(predicate, values ?? new List<object>())
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> AnyAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildSql(predicate, values ?? new List<object>())
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public virtual async ValueTask<long> CountAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await BuildSql(predicate, values ?? new List<object>())
                .LongCountAsync(ct)
                .ConfigureAwait(false);
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

        private static string GetColumnName<TKey>(Expression<Func<TEntity, TKey>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (selector.Body is not MemberExpression key)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return $"{key.Member.Name}";
        }

        private IQueryable<TEntity> BuildSql(string? predicate, List<object> values)
        {
            if (predicate == null)
            {
                return Context.Set<TEntity>();
            }

            var tokens = new List<string>();
            foreach (var token in predicate.Split(' '))
            {
                if (token.StartsWith('@'))
                {
                    tokens.Add($"\"{token.Substring(1)}\"");
                    continue;
                }

                if (token.StartsWith("(@", StringComparison.InvariantCulture))
                {
                    tokens.Add($"(\"{token.Substring(2)}\"");
                    continue;
                }

                tokens.Add(token);
            }

            predicate = string.Join(' ', tokens);

            var sql = $"SELECT * FROM {Context.GetTableName<TEntity>()} WHERE {predicate}";

            return Context.Set<TEntity>().FromSqlRaw(sql, values.ToArray());
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
                : $"({predicate}) AND {filtering}";
        }
    }
}
