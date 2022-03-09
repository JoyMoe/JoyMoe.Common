using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Data;

public abstract class RepositoryBase<TEntity> : IRepository, IRepository<TEntity> where TEntity : class
{
    public bool IgnoreQueryFilters { get; set; }

    #region IRepository

    public async IAsyncEnumerable<object> ListAsync<T>(
        Expression<Func<T, bool>>?                 predicate,
        [EnumeratorCancellation] CancellationToken ct = default) {
        var q = ConvertPredicate(predicate);
        await foreach (var item in ((IRepository<TEntity>)this).ListAsync(q, ct))
        {
            yield return item;
        }
    }

    public async Task<object?> FirstOrDefaultAsync<T>(
        Expression<Func<T, bool>>? predicate,
        CancellationToken          ct = default) {
        var q = ConvertPredicate(predicate);
        return await ((IRepository<TEntity>)this).FirstOrDefaultAsync(q, ct);
    }

    public async Task<object?> SingleOrDefaultAsync<T>(
        Expression<Func<T, bool>>? predicate,
        CancellationToken          ct = default) {
        var q = ConvertPredicate(predicate);
        return await ((IRepository<TEntity>)this).SingleOrDefaultAsync(q, ct);
    }

    public async Task<bool> AnyAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken ct = default) {
        var q = ConvertPredicate(predicate);
        return await ((IRepository<TEntity>)this).AnyAsync(q, ct);
    }

    public async Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken ct = default) {
        var q = ConvertPredicate(predicate);
        return await ((IRepository<TEntity>)this).CountAsync(q, ct);
    }

    public async Task<long> LongCountAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken ct = default) {
        var q = ConvertPredicate(predicate);
        return await ((IRepository<TEntity>)this).LongCountAsync(q, ct);
    }

    public async Task AddAsync(object entity, CancellationToken ct = default) {
        if (entity is not TEntity e) return;
        await ((IRepository<TEntity>)this).AddAsync(e, ct);
    }

    public async Task UpdateAsync(object entity, CancellationToken ct = default) {
        if (entity is not TEntity e) return;
        await ((IRepository<TEntity>)this).UpdateAsync(e, ct);
    }

    public async Task RemoveAsync(object entity, CancellationToken ct = default) {
        if (entity is not TEntity e) return;
        await ((IRepository<TEntity>)this).RemoveAsync(e, ct);
    }

    #endregion

    #region IRepository<TEntity>

    public Expression<Func<TEntity, bool>>? Query(Expression<Func<TEntity, bool>>? predicate = null) {
        return predicate;
    }

    public Task<TEntity?> FindAsync<TKey>(
        Expression<Func<TEntity, TKey>> selector,
        TKey                            id,
        CancellationToken               ct = default) where TKey : struct {
        var key = selector.GetColumn();

        var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTimeOffset.UtcNow.ToFileTime()}");

        var property = Expression.Property(parameter, key.Member.Name);

        var equipment = Expression.Equal(property, Expression.Constant(id));

        var predicate = Expression.Lambda<Func<TEntity, bool>>(equipment, parameter);

        IgnoreQueryFilters = true;

        return SingleOrDefaultAsync(predicate, ct);
    }

    public IAsyncEnumerable<TEntity> FindAllAsync<TKey>(
        Expression<Func<TEntity, TKey>> selector,
        IEnumerable<TKey>               ids,
        CancellationToken               ct = default) where TKey : struct {
        var key = selector.GetColumn();

        var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTimeOffset.UtcNow.ToFileTime()}");

        var property = Expression.Property(parameter, key.Member.Name);

        var contains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                         .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                                         .MakeGenericMethod(typeof(TKey));

        var body      = Expression.Call(contains, Expression.Constant(ids), property);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        IgnoreQueryFilters = true;

        return ListAsync(predicate, selector, ct: ct);
    }

    public abstract IAsyncEnumerable<TEntity> ListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default);

    public abstract IAsyncEnumerable<TEntity> ListAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>>? sort,
        Ordering                         ordering = Ordering.Descending,
        CancellationToken                ct       = default) where TKey : struct;

    public abstract Task<CursorPaginationResponse<TKey, TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        TKey?                            cursor    = null,
        int                              size      = 10,
        Ordering                         ordering  = Ordering.Descending,
        CancellationToken                ct        = default) where TKey : struct;

    public abstract Task<OffsetPaginationResponse<TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        int?                             page      = null,
        int?                             offset    = null,
        int                              size      = 20,
        Ordering                         ordering  = Ordering.Descending,
        CancellationToken                ct        = default) where TKey : struct;

    public abstract Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default);

    public abstract Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default);

    public abstract Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

    public abstract Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

    public abstract Task<long> LongCountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default);

    public virtual Task OnBeforeAddAsync(TEntity entity, CancellationToken ct = default) {
        var now = DateTimeOffset.UtcNow;

        if (entity is ITimestamp stamp)
        {
            stamp.CreationDate     = now;
            stamp.ModificationDate = now;
        }

        if (entity is ISoftDelete soft) soft.DeletionDate = null;

        return Task.CompletedTask;
    }

    public abstract Task AddAsync(TEntity entity, CancellationToken ct = default);

    public virtual Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) {
        var tasks = entities.Select(e => AddAsync(e, ct)).ToArray();

        return Task.WhenAny(tasks);
    }

    public virtual Task OnBeforeUpdateAsync(TEntity entity, CancellationToken ct = default) {
        if (entity is ITimestamp stamp) stamp.ModificationDate = DateTimeOffset.UtcNow;

        return Task.CompletedTask;
    }

    public abstract Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    public virtual Task<bool> OnBeforeRemoveAsync(TEntity entity, CancellationToken ct = default) {
        if (entity is not ISoftDelete soft) return Task.FromResult(true);

        soft.DeletionDate = DateTimeOffset.UtcNow;
        return Task.FromResult(false);
    }

    public abstract Task RemoveAsync(TEntity entity, CancellationToken ct = default);

    public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) {
        var tasks = entities.Select(e => RemoveAsync(e, ct)).ToArray();

        return Task.WhenAny(tasks);
    }

    public abstract Task<int> CommitAsync(CancellationToken ct = default);

    #endregion

    #region Helpers

    protected Expression<Func<TEntity, bool>>? FilteringQuery(Expression<Func<TEntity, bool>>? predicate) {
        if (IgnoreQueryFilters)
        {
            IgnoreQueryFilters = false;

            return predicate;
        }

        if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity))) return predicate;

        var parameter = predicate == null
            ? Expression.Parameter(typeof(ISoftDelete), $"__sd_{DateTimeOffset.UtcNow.ToFileTime()}")
            : predicate.Parameters[0];

        var property  = Expression.Property(parameter, nameof(ISoftDelete.DeletionDate));
        var equipment = Expression.Equal(property, Expression.Constant(null));

        var lambda = Expression.Lambda<Func<TEntity, bool>>(equipment, parameter);

        return predicate.And(lambda);
    }

    private static Expression<Func<TEntity, bool>>? ConvertPredicate<T>(Expression<Func<T, bool>>? predicate) {
        return predicate switch
        {
            null                              => null,
            Expression<Func<TEntity, bool>> e => e,
            _                                 => throw new InvalidOperationException()
        };
    }

    #endregion
}
