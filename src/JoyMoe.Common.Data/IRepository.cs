using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        ValueTask<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default) where TKey : struct;
        IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids) where TKey : struct;
        IAsyncEnumerable<TEntity> ListAsync(string? predicate, List<object>? values, bool everything = false);
        ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey? before = null, int size = 10, string? predicate = null, List<object>? values = null, bool everything = false, CancellationToken ct = default) where TKey : struct, IComparable;
        ValueTask<TEntity?> FirstOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);
        ValueTask<TEntity?> SingleOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);
        ValueTask<bool> AnyAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);
        ValueTask<long> CountAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default);
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
        Task UpdateAsync(TEntity entity, CancellationToken ct = default);
        Task RemoveAsync(TEntity entity, CancellationToken ct = default);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
        ValueTask<int> CommitAsync(CancellationToken ct = default);
    }
}
