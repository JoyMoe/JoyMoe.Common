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
        ValueTask<TEntity?> GetByIdAsync(long id);
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>>? predicate, bool everything = false);
        Task<IEnumerable<TEntity>> PaginateAsync(long? before = null, int size = 10, Expression<Func<TEntity, bool>>? predicate = null, bool everything = false);
        ValueTask<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false);
        ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false);
        ValueTask<int> CountAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false);
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
        Task RemoveAsync(TEntity entity);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);
        ValueTask<int> CommitAsync();
    }
}
