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

        public ValueTask<TEntity> GetByIdAsync(long id)
        {
            return Context.Set<TEntity>().FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return  await Context.Set<TEntity>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> PaginateAsync(int size = 10, long? before = null)
        {
            var query = Context.Set<TEntity>().AsQueryable();

            if (before != null)
            {
                query = query.Where(a => a.Id < before);
            }

            return await query.OrderByDescending(a => a.Id)
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate);
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().SingleOrDefaultAsync(predicate);
        }

        public async Task AddAsync(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await Context.Set<TEntity>().AddRangeAsync(entities).ConfigureAwait(false);
        }

        public void Update(TEntity entity)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }

        public int Commit()
        {
            return Context.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
