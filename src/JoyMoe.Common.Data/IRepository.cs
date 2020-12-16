using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        ValueTask<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default) where TKey: struct;
        IAsyncEnumerable<TEntity> FindAllAsync<TKey>(Expression<Func<TEntity, TKey>> selector, IEnumerable<TKey> ids) where TKey : struct;
        IAsyncEnumerable<TEntity> ListAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false);
        ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey? before = null, int size = 10, Expression<Func<TEntity, bool>>? predicate = null, bool everything = false, CancellationToken ct = default) where TKey : struct, IComparable;
        ValueTask<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false, CancellationToken ct = default);
        ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false, CancellationToken ct = default);
        ValueTask<int> CountAsync(Expression<Func<TEntity, bool>>? predicate, bool everything = false, CancellationToken ct = default);
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
        Task UpdateAsync(TEntity entity, CancellationToken ct = default);
        Task RemoveAsync(TEntity entity, CancellationToken ct = default);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
        ValueTask<int> CommitAsync(CancellationToken ct = default);
    }
}
