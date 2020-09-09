using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;

namespace JoyMoe.Common.EntityFrameworkCore.Repositories
{
    public interface IRepository<TEntity> where TEntity : class, IDataEntity
    {
        IQueryable<TEntity> AsQueryable();
        ValueTask<TEntity> GetByIdAsync(long id);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        int Commit();
        Task<int> CommitAsync();
    }
}