using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Data;

public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
{
    public bool IgnoreQueryFilters { get; set; }

    public Task<TEntity?> FindAsync<TKey>(
        Expression<Func<TEntity, TKey>> selector,
        TKey                            id,
        CancellationToken               ct = default)
        where TKey : struct
    {
        var key = selector.GetColumn();

        var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}");

        var property = Expression.Property(parameter, key.Member.Name);

        var equipment = Expression.Equal(property, Expression.Constant(id));

        var predicate = Expression.Lambda<Func<TEntity, bool>>(equipment, parameter);

        IgnoreQueryFilters = true;

        return SingleOrDefaultAsync(predicate, ct);
    }

    public IAsyncEnumerable<TEntity> FindAllAsync<TKey>(
        Expression<Func<TEntity, TKey>> selector,
        IEnumerable<TKey>               ids,
        CancellationToken               ct = default)
        where TKey : struct
    {
        if (ids == null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        var key = selector.GetColumn();

        var parameter = Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}");

        var property = Expression.Property(parameter, key.Member.Name);

        var contains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                         .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                                         .MakeGenericMethod(typeof(TKey));

        var body      = Expression.Call(contains, Expression.Constant(ids), property);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        IgnoreQueryFilters = true;

        return ListAsync(predicate, selector, ct);
    }

    public abstract IAsyncEnumerable<TEntity> ListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default);

    public abstract IAsyncEnumerable<TEntity> ListAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>>? ordering,
        CancellationToken                ct = default)
        where TKey : struct;

    public abstract Task<CursorPaginationResponse<TKey, TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        TKey?                            cursor    = null,
        int                              size      = 20,
        CancellationToken                ct        = default)
        where TKey : struct;

    public abstract Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default);

    public abstract Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default);

    public abstract Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

    public abstract Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

    public virtual Task OnBeforeAddAsync(TEntity entity, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        if (entity is ITimestamp stamp)
        {
            stamp.CreationDate     = now;
            stamp.ModificationDate = now;
        }

        if (entity is ISoftDelete soft)
        {
            soft.DeletionDate = null;
        }

        return Task.CompletedTask;
    }

    public abstract Task AddAsync(TEntity entity, CancellationToken ct = default);

    public virtual Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        var tasks = entities.Select(e => AddAsync(e, ct)).ToArray();

        return Task.WhenAny(tasks);
    }

    public virtual Task OnBeforeUpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity is ITimestamp stamp)
        {
            stamp.ModificationDate = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    public abstract Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    public virtual Task<bool> OnBeforeRemoveAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity is ISoftDelete soft)
        {
            soft.DeletionDate = DateTime.UtcNow;

            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public abstract Task RemoveAsync(TEntity entity, CancellationToken ct = default);

    public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        var tasks = entities.Select(e => RemoveAsync(e, ct)).ToArray();

        return Task.WhenAny(tasks);
    }

    public abstract Task<int> CommitAsync(CancellationToken ct = default);

    protected Expression<Func<TEntity, bool>>? FilteringQuery(Expression<Func<TEntity, bool>>? predicate)
    {
        if (IgnoreQueryFilters)
        {
            IgnoreQueryFilters = false;

            return predicate;
        }

        if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            return predicate;
        }

        var parameter = predicate == null
            ? Expression.Parameter(typeof(ISoftDelete), $"__sd_{DateTime.Now.ToFileTime()}")
            : predicate.Parameters[0];

        var property  = Expression.Property(parameter, nameof(ISoftDelete.DeletionDate));
        var equipment = Expression.Equal(property, Expression.Constant(null));

        var lambda = Expression.Lambda<Func<TEntity, bool>>(equipment, parameter);

        return predicate.And(lambda);
    }
}
