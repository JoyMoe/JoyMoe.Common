using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JoyMoe.Common.Abstractions;
using LinqToDB;

namespace JoyMoe.Common.Data.LinqToDB;

public class LinQ2DbRepository<TEntity> : LinQ2DbRepository<DataContext, TEntity> where TEntity : class
{
    public LinQ2DbRepository(DataContext context) : base(context) { }
}

public class LinQ2DbRepository<TContext, TEntity> : RepositoryBase<TEntity> where TContext : DataContext
                                                                            where TEntity : class
{
    protected TContext                Context      { get; }
    protected DataContextTransaction? Transaction  { get; set; }
    protected int                     RowsAffected { get; set; }

    public LinQ2DbRepository(TContext context) {
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

        query = ordering switch
        {
            Ordering.Descending when sort != null => query.OrderByDescending(sort),
            Ordering.Ascending when sort != null => query.OrderBy(sort),
            _ => throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null)
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

        query = ordering switch
        {
            Ordering.Descending => query.OrderByDescending(selector),
            Ordering.Ascending  => query.OrderBy(selector),
            _                   => throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null)
        };

        var data = await query.Take(size + 1).ToArrayAsync(ct);

        return new CursorPaginationResponse<TKey, TEntity>
        {
            Next = data.Length > size ? converter(data.Last()) : null,
            Data = data.Length > size ? data[..size] : data
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

        query = ordering switch
        {
            Ordering.Descending => query.OrderByDescending(selector),
            Ordering.Ascending  => query.OrderBy(selector),
            _                   => throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null)
        };

        if (page.HasValue)
        {
            offset = (page - 1) * size;
        }
        else if (offset.HasValue)
        {
            page = offset / size + 1;
        }
        else
        {
            throw new ArgumentNullException(nameof(offset));
        }

        var data = await query.Skip(offset.Value).Take(size).ToArrayAsync(ct);

        return new OffsetPaginationResponse<TEntity> { Size = count, Page = page.Value, Data = data };
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
        await BeginTransactionAsync();

        await OnBeforeAddAsync(entity, ct);

        RowsAffected += await Context.InsertAsync(entity, token: ct);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync();

        await OnBeforeUpdateAsync(entity, ct);

        RowsAffected += await Context.UpdateAsync(entity, token: ct);
    }

    public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync();

        if (await OnBeforeRemoveAsync(entity, ct))
        {
            RowsAffected += await Context.DeleteAsync(entity, token: ct);
            return;
        }

        RowsAffected += await Context.UpdateAsync(entity, token: ct);
    }

    public override async Task<int> CommitAsync(CancellationToken ct = default) {
        if (Transaction == null) return 0;

        try
        {
            await Transaction.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            await Transaction.RollbackTransactionAsync(ct);

            throw new TransactionAbortedException(ex.Message, ex);
        }

        var rows = RowsAffected;

        Transaction  = null;
        RowsAffected = 0;

        return rows;
    }

    private static IQueryable<TEntity> BuildQuery(TContext context, Expression<Func<TEntity, bool>>? predicate) {
        return predicate != null ? context.GetTable<TEntity>().Where(predicate) : context.GetTable<TEntity>();
    }

    private async Task BeginTransactionAsync() {
        if (Transaction != null) return;

        Transaction = await Context.BeginTransactionAsync();
    }
}
