using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Transactions;
using JoyMoe.Common.Abstractions;
using LinqToDB;
using LinqToDB.Data;

namespace JoyMoe.Common.Data.LinqToDB;

public class LinQ2DbRepository<TContext, TEntity> : RepositoryBase<TEntity>
    where TContext : DataConnection
    where TEntity : class
{
    protected TContext                   Context      { get; }
    protected DataConnectionTransaction? Transaction  { get; set; }
    protected int                        RowsAffected { get; set; }

    public string TableName => nameof(TEntity).Pluralize();

    public LinQ2DbRepository(TContext context) {
        Context = context;
    }

    public override async IAsyncEnumerable<TEntity> ListAsync(
        Expression<Func<TEntity, bool>>?           predicate,
        [EnumeratorCancellation] CancellationToken ct = default) {
        var enumerable = BuildQuery(predicate).AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var entity in enumerable) yield return entity;
    }

    public override async IAsyncEnumerable<TEntity> ListAsync<TKey>(
        Expression<Func<TEntity, bool>>?           predicate,
        Expression<Func<TEntity, TKey>>?           sort,
        Ordering                                   ordering = Ordering.Descending,
        [EnumeratorCancellation] CancellationToken ct       = default) {
        var query = BuildQuery(predicate);

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

        var key = selector.GetColumn();

        var parameter = predicate == null ? Expression.Parameter(typeof(TEntity)) : predicate.Parameters[0];

        var property = Expression.Property(parameter, key.Member.Name);

        Expression<Func<TEntity, bool>>? filtering = null;
        if (cursor.HasValue) {
            var than = Expression.LessThanOrEqual(property, Expression.Constant(cursor));
            filtering = Expression.Lambda<Func<TEntity, bool>>(than, parameter);
        }

        var query = BuildQuery(predicate.And(filtering));

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
        var query = BuildQuery(predicate);

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
        return await BuildQuery(predicate).FirstOrDefaultAsync(ct);
    }

    public override async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).SingleOrDefaultAsync(ct);
    }

    public override async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).AnyAsync(ct);
    }

    public override async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).CountAsync(ct);
    }

    public override async Task<long> LongCountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).LongCountAsync(ct);
    }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync(ct);

        await OnBeforeAddAsync(entity, ct);

        RowsAffected += await Context.InsertAsync(entity, tableName: TableName, token: ct);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync(ct);

        await OnBeforeUpdateAsync(entity, ct);

        RowsAffected += await Context.UpdateAsync(entity, tableName: TableName, token: ct);
    }

    public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync(ct);

        if (await OnBeforeRemoveAsync(entity, ct)) {
            RowsAffected += await Context.DeleteAsync(entity, tableName: TableName, token: ct);
            return;
        }

        RowsAffected += await Context.UpdateAsync(entity, tableName: TableName, token: ct);
    }

    public override async Task<int> CommitAsync(CancellationToken ct = default) {
        if (Transaction == null) return 0;

        try {
            await Transaction.CommitAsync(ct);
        } catch (Exception ex) {
            await Transaction.RollbackAsync(ct);

            throw new TransactionAbortedException(ex.Message, ex);
        }

        var rows = RowsAffected;

        Transaction  = null;
        RowsAffected = 0;

        return rows;
    }

    private IQueryable<TEntity> BuildQuery(Expression<Func<TEntity, bool>>? predicate) {
        predicate = FilteringQuery(predicate);

        var table = Context.GetTable<TEntity>().TableName(nameof(TEntity).Pluralize());

        return predicate != null ? table.Where(predicate) : table;
    }

    private async Task BeginTransactionAsync(CancellationToken ct) {
        if (Transaction != null) return;

        Transaction = await Context.BeginTransactionAsync(ct);
    }
}
