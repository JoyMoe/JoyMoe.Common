using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Data;

public interface IRepository
{
    bool IgnoreQueryFilters { get; set; }

    #region List

    IAsyncEnumerable<object> ListAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken ct = default);

    #endregion

    #region Query

    Task<object?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>>?  predicate, CancellationToken ct = default);
    Task<object?> SingleOrDefaultAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken ct = default);
    Task<bool>    AnyAsync<T>(Expression<Func<T, bool>>?             predicate, CancellationToken ct = default);
    Task<int>     CountAsync<T>(Expression<Func<T, bool>>?           predicate, CancellationToken ct = default);
    Task<long>    LongCountAsync<T>(Expression<Func<T, bool>>?       predicate, CancellationToken ct = default);

    #endregion

    #region Update

    Task AddAsync(object    entity, CancellationToken ct = default);
    Task UpdateAsync(object entity, CancellationToken ct = default);
    Task RemoveAsync(object entity, CancellationToken ct = default);

    #endregion

    Task<int> CommitAsync(CancellationToken ct = default);
}

public interface IRepository<TEntity> where TEntity : class
{
    bool IgnoreQueryFilters { get; set; }

    Expression<Func<TEntity, bool>>? Query(Expression<Func<TEntity, bool>>? predicate = null);

    #region List

    Task<TEntity?> FindAsync<TKey>(Expression<Func<TEntity, TKey>> selector, TKey id, CancellationToken ct = default)
        where TKey : struct;

    IAsyncEnumerable<TEntity> FindAllAsync<TKey>(
        Expression<Func<TEntity, TKey>> selector,
        IEnumerable<TKey>               ids,
        CancellationToken               ct = default) where TKey : struct;

    IAsyncEnumerable<TEntity> ListAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

    IAsyncEnumerable<TEntity> ListAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>>? sort,
        Ordering                         ordering = Ordering.Descending,
        CancellationToken                ct       = default) where TKey : struct;

    Task<CursorPaginationResponse<TKey, TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        TKey?                            cursor    = null,
        int                              size      = 10,
        Ordering                         ordering  = Ordering.Descending,
        CancellationToken                ct        = default) where TKey : struct;

    Task<OffsetPaginationResponse<TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        int?                             page      = null,
        int?                             offset    = null,
        int                              size      = 20,
        Ordering                         ordering  = Ordering.Descending,
        CancellationToken                ct        = default) where TKey : struct;

    #endregion

    #region Query

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>?  predicate, CancellationToken ct = default);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);
    Task<bool>     AnyAsync(Expression<Func<TEntity, bool>>?             predicate, CancellationToken ct = default);
    Task<int>      CountAsync(Expression<Func<TEntity, bool>>?           predicate, CancellationToken ct = default);
    Task<long>     LongCountAsync(Expression<Func<TEntity, bool>>?       predicate, CancellationToken ct = default);

    #endregion

    #region Update

    Task AddAsync(TEntity                   entity,   CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    Task RemoveAsync(TEntity                   entity,   CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    #endregion

    Task<int> CommitAsync(CancellationToken ct = default);
}
