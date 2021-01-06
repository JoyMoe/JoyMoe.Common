using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public bool IgnoreQueryFilters { get; set; }

        public virtual Task<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id)
            where TKey : struct
        {
            IgnoreQueryFilters = true;

            return SingleOrDefaultAsync($"@{selector.GetColumnName()} = {{0}}", id);
        }

        public virtual IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids)
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

            IgnoreQueryFilters = true;

            return ListAsync($"@{selector.GetColumnName()} IN ( {list} )", keys.ToArray());
        }

        public abstract IAsyncEnumerable<TEntity> ListAsync(string? predicate = null, params object[] values);

        public abstract Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            string? predicate = null,
            params object[] values)
            where TKey : struct, IComparable;

        public abstract Task<TEntity?> FirstOrDefaultAsync(string? predicate = null, params object[] values);

        public abstract Task<TEntity?> SingleOrDefaultAsync(string? predicate = null, params object[] values);

        public abstract Task<bool> AnyAsync(string? predicate = null, params object[] values);

        public abstract Task<long> CountAsync(string? predicate = null, params object[] values);

        public virtual Task OnBeforeAddAsync(TEntity entity)
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

        public abstract Task AddAsync(TEntity entity);

        public virtual Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var tasks = entities.Select(AddAsync).ToArray();

            return Task.WhenAny(tasks);
        }

        public virtual Task OnBeforeUpdateAsync(TEntity entity)
        {
            if (entity is ITimestamp stamp)
            {
                stamp.UpdatedAt = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }

        public abstract Task UpdateAsync(TEntity entity);

        public virtual Task<bool> OnBeforeRemoveAsync(TEntity entity)
        {
            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = DateTime.UtcNow;

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public abstract Task RemoveAsync(TEntity entity);

        public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var tasks = entities.Select(RemoveAsync).ToArray();

            return Task.WhenAny(tasks);
        }

        public abstract Task<int> CommitAsync();

        protected string? FilteringQuery(string? predicate)
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

            var filtering = $"@{nameof(ISoftDelete.DeletedAt)} IS NULL";

            return string.IsNullOrEmpty(predicate)
                ? filtering
                : $"( {predicate} ) AND {filtering}";
        }
    }
}
