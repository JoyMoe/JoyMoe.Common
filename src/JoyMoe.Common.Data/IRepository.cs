using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        bool IgnoreQueryFilters { get; set; }

        Task<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id) where TKey : struct;
        IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids) where TKey : struct;
        IAsyncEnumerable<TEntity> ListAsync(string? predicate = null, params object[] values);
        Task<IEnumerable<TEntity>> PaginateAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey? before = null, int size = 10, string? predicate = null, params object[] values) where TKey : struct, IComparable;
        Task<TEntity?> FirstOrDefaultAsync(string? predicate = null, params object[] values);
        Task<TEntity?> SingleOrDefaultAsync(string? predicate = null, params object[] values);
        Task<bool> AnyAsync(string? predicate = null, params object[] values);
        Task<long> CountAsync(string? predicate = null, params object[] values);
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
        Task RemoveAsync(TEntity entity);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);
        Task<int> CommitAsync();
    }
}
