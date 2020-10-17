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

        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>>? predicate)
        {
            var query = Context.Set<TEntity>().AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return query;
        }

        public virtual async Task<IEnumerable<TEntity>> PaginateAsync(long? before = null, int size = 10, Expression<Func<TEntity, bool>>? predicate = null)
        {
            var query = Find(predicate);

            if (before != null)
            {
                query = query.Where(a => a.Id < before);
            }

            return await query.OrderByDescending(a => a.Id)
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public virtual async ValueTask<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().SingleOrDefaultAsync(predicate).ConfigureAwait(false);
        }

        public async ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().AnyAsync(predicate).ConfigureAwait(false);
        }

        public async ValueTask<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().CountAsync(predicate).ConfigureAwait(false);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await Context.AddAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await Context.AddRangeAsync(entities).ConfigureAwait(false);
        }

        public virtual void Update(TEntity entity)
        {
            Context.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            Context.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.RemoveRange(entities);
        }

        public virtual int Commit()
        {
            return Context.SaveChanges();
        }

        public virtual async ValueTask<int> CommitAsync()
        {
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
