using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default) where TKey : struct;
        IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids, CancellationToken ct = default) where TKey : struct;
        IAsyncEnumerable<TEntity> ListAsync(bool everything = false, string? predicate = null, CancellationToken ct = default, params object[] values);
        Task<IEnumerable<TEntity>> PaginateAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey? before = null, int size = 10, bool everything = false, string? predicate = null, CancellationToken ct = default, params object[] values) where TKey : struct, IComparable;
        Task<TEntity?> FirstOrDefaultAsync(bool everything = false, string? predicate = null, CancellationToken ct = default, params object[] values);
        Task<TEntity?> SingleOrDefaultAsync(bool everything = false, string? predicate = null, CancellationToken ct = default, params object[] values);
        Task<bool> AnyAsync(bool everything = false, string? predicate = null, CancellationToken ct = default, params object[] values);
        Task<long> CountAsync(bool everything = false, string? predicate = null, CancellationToken ct = default, params object[] values);
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
        Task UpdateAsync(TEntity entity, CancellationToken ct = default);
        Task RemoveAsync(TEntity entity, CancellationToken ct = default);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
        Task<int> CommitAsync(CancellationToken ct = default);
    }
}
