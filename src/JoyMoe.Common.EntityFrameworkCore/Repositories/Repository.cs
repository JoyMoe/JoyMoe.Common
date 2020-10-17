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
        protected DbContextBase Context { get; }

        public Repository(DbContextBase context)
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

        public virtual async Task AddAsync(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await Context.Set<TEntity>().AddRangeAsync(entities).ConfigureAwait(false);
        }

        public virtual void Update(TEntity entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
            {
                Context.Set<TEntity>().Attach(entity);
            }

            Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
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
