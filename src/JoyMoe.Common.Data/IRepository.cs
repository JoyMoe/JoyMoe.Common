using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Data;

public interface IRepository<TEntity> where TEntity : class
{
    bool IgnoreQueryFilters { get; set; }

    Task<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default)
        where TKey : struct;

    IAsyncEnumerable<TEntity> FindAllAsync<TKey>(
        Expression<Func<TEntity, TKey>> selector,
        IEnumerable<TKey>               ids,
        CancellationToken               ct = default) where TKey : struct;

    IAsyncEnumerable<TEntity> ListAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

    IAsyncEnumerable<TEntity> ListAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>>? ordering,
        CancellationToken                ct = default) where TKey : struct;

    Task<CursorPaginationResponse<TKey, TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        TKey?                            cursor    = null,
        int                              size      = 10,
        CancellationToken                ct        = default) where TKey : struct;

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>?  predicate, CancellationToken ct = default);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>?                 predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>?                predicate, CancellationToken ct = default);
    Task<long> LongCountAsync(Expression<Func<TEntity, bool>>?           predicate, CancellationToken ct = default);

    Task AddAsync(TEntity                   entity,   CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    Task RemoveAsync(TEntity                   entity,   CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    Task<int> CommitAsync(CancellationToken ct = default);
}
