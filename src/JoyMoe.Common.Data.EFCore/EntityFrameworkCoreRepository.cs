using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JoyMoe.Common.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.Data.EFCore;

public class EntityFrameworkCoreRepository<TContext, TEntity> : RepositoryBase<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    protected TContext Context { get; }

    public EntityFrameworkCoreRepository(TContext context) {
        Context = context;
    }

    public override async IAsyncEnumerable<TEntity> ListAsync(
        Expression<Func<TEntity, bool>>?           predicate,
        [EnumeratorCancellation] CancellationToken ct = default) {
        predicate = FilteringQuery(predicate);

        var enumerable = BuildQuery(Context, predicate).AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var entity in enumerable) yield return entity;
    }

    public override async IAsyncEnumerable<TEntity> ListAsync<TKey>(
        Expression<Func<TEntity, bool>>?           predicate,
        Expression<Func<TEntity, TKey>>?           sort,
        Ordering                                   ordering = Ordering.Descending,
        [EnumeratorCancellation] CancellationToken ct       = default) {
        predicate = FilteringQuery(predicate);

        var query = BuildQuery(Context, predicate);

        query = ordering switch {
            Ordering.Descending when sort != null => query.OrderByDescending(sort),
            Ordering.Ascending when sort != null => query.OrderBy(sort),
            _ => throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null),
        };

        var enumerable = query.AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var entity in enumerable) yield return entity;
    }

    public override async Task<CursorPaginationResponse<TKey, TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        TKey?                            cursor    = null,
        int                              size      = 10,
        Ordering                         ordering  = Ordering.Descending,
        CancellationToken                ct        = default) {
        var converter = selector.Compile();

        predicate = FilteringQuery(predicate);

        var key = selector.GetColumn();

        var parameter = predicate == null ? Expression.Parameter(typeof(TEntity)) : predicate.Parameters[0];

        var property = Expression.Property(parameter, key.Member.Name);

        Expression<Func<TEntity, bool>>? filtering = null;
        if (cursor.HasValue) {
            var than = Expression.LessThanOrEqual(property, Expression.Constant(cursor));
            filtering = Expression.Lambda<Func<TEntity, bool>>(than, parameter);
        }

        var query = BuildQuery(Context, predicate.And(filtering));

        query = ordering switch {
            Ordering.Descending => query.OrderByDescending(selector),
            Ordering.Ascending  => query.OrderBy(selector),
            _                   => throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null),
        };

        var data = await query.Take(size + 1).ToArrayAsync(ct);

        return new CursorPaginationResponse<TKey, TEntity> {
            Next = data.Length > size ? converter(data.Last()) : null, Data = data.Length > size ? data[..size] : data,
        };
    }

    public override async Task<OffsetPaginationResponse<TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        int?                             page      = null,
        int?                             offset    = null,
        int                              size      = 20,
        Ordering                         ordering  = Ordering.Descending,
        CancellationToken                ct        = default) {
        predicate = FilteringQuery(predicate);

        var query = BuildQuery(Context, predicate);

        var count = await query.CountAsync(ct);

        query = ordering switch {
            Ordering.Descending => query.OrderByDescending(selector),
            Ordering.Ascending  => query.OrderBy(selector),
            _                   => throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null),
        };

        if (page.HasValue) {
            offset = (page - 1) * size;
        } else if (offset.HasValue) {
            page = offset / size + 1;
        } else {
            page   = 1;
            offset = 0;
        }

        var data = await query.Skip(offset.Value).Take(size).ToArrayAsync(ct);

        return new OffsetPaginationResponse<TEntity> {
            Total = count,
            Page  = page.Value,
            Data  = data,
        };
    }

    public override async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate).FirstOrDefaultAsync(ct);
    }

    public override async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate).SingleOrDefaultAsync(ct);
    }

    public override async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate).AnyAsync(ct);
    }

    public override async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate).CountAsync(ct);
    }

    public override async Task<long> LongCountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        predicate = FilteringQuery(predicate);

        return await BuildQuery(Context, predicate).LongCountAsync(ct);
    }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default) {
        await OnBeforeAddAsync(entity, ct);

        await Context.AddAsync(entity, ct);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default) {
        await OnBeforeUpdateAsync(entity, ct);

        Context.Entry(entity).State = EntityState.Detached;
        Context.Update(entity);
    }

    public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default) {
        if (await OnBeforeRemoveAsync(entity, ct)) {
            Context.Remove(entity);
            return;
        }

        Context.Entry(entity).State = EntityState.Detached;
        Context.Update(entity);
    }

    public override async Task<int> CommitAsync(CancellationToken ct = default) {
        return await Context.SaveChangesAsync(ct);
    }

    private static IQueryable<TEntity> BuildQuery(TContext context, Expression<Func<TEntity, bool>>? predicate) {
        return predicate != null ? context.Set<TEntity>().Where(predicate) : context.Set<TEntity>();
    }
}
