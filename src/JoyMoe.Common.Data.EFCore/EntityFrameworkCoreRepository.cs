using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.Data.EFCore;

public class EntityFrameworkCoreRepository<TEntity> : EntityFrameworkCoreRepository<DbContext, TEntity>
    where TEntity : class
{
    public EntityFrameworkCoreRepository(DbContext context) : base(context)
    {
    }
}

public class EntityFrameworkCoreRepository<TContext, TEntity> : RepositoryBase<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    protected TContext Context { get; }

    public EntityFrameworkCoreRepository(TContext context)
    {
        Context = context;
    }

    public override async IAsyncEnumerable<TEntity> ListAsync(
        Expression<Func<TEntity, bool>>?           predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        predicate = FilteringQuery(predicate);

        var enumerable = BuildQuery(Context, predicate)
                        .AsAsyncEnumerable()
                        .WithCancellation(ct);

        await foreach (var entity in enumerable)
        {
            yield return entity;
        }
    }

    public override async IAsyncEnumerable<TEntity> ListAsync<TKey>(
        Expression<Func<TEntity, bool>>?           predicate,
        Expression<Func<TEntity, TKey>>?           ordering,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        predicate = FilteringQuery(predicate);

        var query = BuildQuery(Context, predicate);

        if (ordering != null)
        {
            query = query.OrderByDescending(ordering);
        }

        var enumerable = query
                        .AsAsyncEnumerable()
                        .WithCancellation(ct);

        await foreach (var entity in enumerable)
        {
            yield return entity;
        }
    }

    public override async Task<CursorPaginationResponse<TKey, TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        TKey?                            cursor    = null,
        int                              size      = 20,
        CancellationToken                ct        = default)
    {
        var converter = selector.Compile();

        predicate = FilteringQuery(predicate);

        var key = selector.GetColumn();

        var parameter = predicate == null
            ? Expression.Parameter(typeof(TEntity), $"__de_{DateTime.Now.ToFileTime()}")
            : predicate.Parameters[0];

        var property = Expression.Property(parameter, key.Member.Name);

        Expression<Func<TEntity, bool>>? filtering = null;
        if (cursor.HasValue)
        {
            var than = Expression.LessThanOrEqual(property, Expression.Constant(cursor));
            filtering = Expression.Lambda<Func<TEntity, bool>>(than, parameter);
        }

        var query = BuildQuery(Context, predicate.And(filtering));

        var data = await query
                        .OrderByDescending(selector)
                        .Take(size + 1)
                        .ToArrayAsync(ct);

        return new CursorPaginationResponse<TKey, TEntity>
        {
            Next = data?.Length > size ? converter(data.Last()) : null,
            Data = data?.Length > size ? data[..size] : data
        };
    }

    public override async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default)
    {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate)
                    .FirstOrDefaultAsync(ct)
                    .ConfigureAwait(false);
    }

    public override async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default)
    {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate)
                    .SingleOrDefaultAsync(ct)
                    .ConfigureAwait(false);
    }

    public override async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default)
    {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate)
                    .AnyAsync(ct)
                    .ConfigureAwait(false);
    }

    public override async Task<long> CountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default)
    {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate)
                    .LongCountAsync(ct)
                    .ConfigureAwait(false);
    }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await OnBeforeAddAsync(entity, ct).ConfigureAwait(false);

        await Context.AddAsync(entity, ct).ConfigureAwait(false);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await OnBeforeUpdateAsync(entity, ct).ConfigureAwait(false);

        Context.Entry(entity).State = EntityState.Detached;
        Context.Update(entity);
    }

    public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        if (await OnBeforeRemoveAsync(entity, ct).ConfigureAwait(false))
        {
            Context.Remove(entity);
            return;
        }

        Context.Entry(entity).State = EntityState.Detached;
        Context.Update(entity);
    }

    public override async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await Context.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static IQueryable<TEntity> BuildQuery(TContext context, Expression<Func<TEntity, bool>>? predicate)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return predicate != null
            ? context.Set<TEntity>().Where(predicate)
            : context.Set<TEntity>();
    }
}
