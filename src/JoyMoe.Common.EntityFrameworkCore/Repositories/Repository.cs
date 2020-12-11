using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.EntityFrameworkCore.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IDataEntity
    {
        protected DbContext Context { get; }

        public Repository(DbContext context)
        {
            Context = context;
        }

        public virtual async ValueTask<TEntity?> GetByIdAsync(long id)
        {
            return await Context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
        }

        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>>? predicate, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);
            return predicate == null
                ? Context.Set<TEntity>()
                : Context.Set<TEntity>().Where(predicate);
        }

        public virtual async Task<IEnumerable<TEntity>> PaginateAsync(long? before = null, int size = 10, Expression<Func<TEntity, bool>>? predicate = null, bool everything = false)
        {
            var query = Find(predicate, everything);

            if (before != null)
            {
                query = query.Where(a => a.Id < before);
            }

            return await query.OrderByDescending(a => a.Id)
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public virtual async ValueTask<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);
            return predicate == null
                ? await Context.Set<TEntity>().SingleOrDefaultAsync().ConfigureAwait(false)
                : await Context.Set<TEntity>().SingleOrDefaultAsync(predicate).ConfigureAwait(false);
        }

        public async ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);
            return predicate == null
                ? await Context.Set<TEntity>().AnyAsync().ConfigureAwait(false)
                : await Context.Set<TEntity>().AnyAsync(predicate).ConfigureAwait(false);
        }

        public async ValueTask<int> CountAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);
            return predicate == null
                ? await Context.Set<TEntity>().CountAsync().ConfigureAwait(false)
                : await Context.Set<TEntity>().CountAsync(predicate).ConfigureAwait(false);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var now = DateTime.UtcNow;

            entity.CreatedAt = now;
            entity.UpdatedAt = now;

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = null;
            }

            await Context.AddAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                await AddAsync(entity).ConfigureAwait(false);
            }
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.UpdatedAt = DateTime.UtcNow;

            Context.Entry(entity).State = EntityState.Detached;
            Context.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task RemoveAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = DateTime.UtcNow;
                return Task.CompletedTask;
            }

            Context.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                await RemoveAsync(entity).ConfigureAwait(false);
            }
        }

        public virtual async ValueTask<int> CommitAsync()
        {
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static Expression<Func<TEntity,bool>>? FilteringQuery(Expression<Func<TEntity, bool>>? predicate, bool everything)
        {
            if (everything) return predicate;

            var parameter = predicate == null
                ? Expression.Parameter(typeof(ISoftDelete), "sd")
                : predicate.Parameters[0];

            var property = Expression.Property(parameter, nameof(ISoftDelete.DeletedAt));
            var equipment = Expression.Equal(property, Expression.Constant(null));

            return predicate == null
                ? Expression.Lambda<Func<TEntity, bool>>(equipment, parameter)
                : Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(predicate.Body, equipment), parameter);
        }
    }
}
