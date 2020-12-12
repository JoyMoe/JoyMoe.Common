using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        ValueTask<TEntity> GetByIdAsync(long id);
        IAsyncEnumerable<TEntity> ListAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false);
        ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey? before = null, int size = 10, Expression<Func<TEntity, bool>>? predicate = null, bool everything = false) where TKey : struct, IComparable;
        ValueTask<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false);
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
