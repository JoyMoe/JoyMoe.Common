using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Dapper.Contrib;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Data.Dapper;

public class DapperRepository<TEntity> : RepositoryBase<TEntity> where TEntity : class
{
    protected DbConnection   Connection  { get; }
    protected DbTransaction? Transaction { get; set; }

    public DapperRepository(DbConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public override async IAsyncEnumerable<TEntity> ListAsync(
        Expression<Func<TEntity, bool>>?           predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        predicate = FilteringQuery(predicate);

        var entities = await Connection.QueryAsync(predicate).ConfigureAwait(false);
        if (entities == null) yield break;

        foreach (var entity in entities)
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

        Dictionary<string, string?>? orderings = null;
        if (!string.IsNullOrWhiteSpace(ordering?.GetColumn().Member.Name))
        {
            orderings = new Dictionary<string, string?>
            {
                [ordering.GetColumn().Member.Name] = null
            };
        }

        var entities = await Connection.QueryAsync(predicate, orderings).ConfigureAwait(false);
        if (entities == null) yield break;

        foreach (var entity in entities)
        {
            yield return entity;
        }
    }

    public override async Task<PaginationResponse<TKey, TEntity>> PaginateAsync<TKey>(
        Expression<Func<TEntity, TKey>>  selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        TKey?                            cursor    = null,
        int                              size      = 20,
        CancellationToken                ct        = default)
    {
        var func = selector.Compile();

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

        var entities = await Connection.QueryAsync(predicate.And(filtering), new Dictionary<string, string?>
        {
            [selector.GetColumn().Member.Name] = "DESC"
        }, size + 1).ConfigureAwait(false);
        var data = entities.ToArray();

        TEntity? prev  = null;
        var      first = data.FirstOrDefault();
        if (first != null)
        {
            var than    = Expression.GreaterThan(property, Expression.Constant(func(first)));
            var greater = Expression.Lambda<Func<TEntity, bool>>(than, parameter);

            entities = await Connection.QueryAsync(predicate.And(greater), new Dictionary<string, string?>
            {
                [selector.GetColumn().Member.Name] = null
            }, size).ConfigureAwait(false);

            prev = entities.LastOrDefault();
        }

        return new PaginationResponse<TKey, TEntity>
        {
            Prev = prev != null ? func(prev) : null,
            Next = data.Length > size ? func(data.Last()) : null,
            Data = data.Length > size ? data[..size] : data
        };
    }

    public override async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate,
                                                             CancellationToken                ct = default)
    {
        predicate = FilteringQuery(predicate);

        return await Connection.QueryFirstOrDefaultAsync(predicate).ConfigureAwait(false);
    }

    public override async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate,
                                                              CancellationToken                ct = default)
    {
        predicate = FilteringQuery(predicate);

        return await Connection.QuerySingleOrDefaultAsync(predicate).ConfigureAwait(false);
    }

    public override async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate,
                                              CancellationToken                ct = default)
    {
        var count = await CountAsync(predicate, ct).ConfigureAwait(false);

        return count > 0;
    }

    public override async Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate,
                                                CancellationToken                ct = default)
    {
        predicate = FilteringQuery(predicate);

        return await Connection.CountAsync(predicate).ConfigureAwait(false);
    }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await BeginTransactionAsync().ConfigureAwait(false);

        await OnBeforeAddAsync(entity, ct).ConfigureAwait(false);

        await Connection.InsertAsync(entity, Transaction).ConfigureAwait(false);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await BeginTransactionAsync().ConfigureAwait(false);

        await OnBeforeUpdateAsync(entity, ct).ConfigureAwait(false);

        await Connection.UpdateAsync(entity, Transaction).ConfigureAwait(false);
    }

    public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await BeginTransactionAsync().ConfigureAwait(false);

        if (await OnBeforeRemoveAsync(entity, ct).ConfigureAwait(false))
        {
            await Connection.DeleteAsync(entity, Transaction).ConfigureAwait(false);
            return;
        }

        await Connection.UpdateAsync(entity, Transaction).ConfigureAwait(false);
    }

    public override async Task<int> CommitAsync(CancellationToken ct = default)
    {
        if (Transaction == null) return 0;

        try
        {
            await Transaction.CommitAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Transaction.RollbackAsync(ct).ConfigureAwait(false);

            throw new TransactionAbortedException(ex.Message, ex);
        }

        Transaction = null;

        // TODO: Get count of changes

        return 1;
    }

    private async Task BeginTransactionAsync()
    {
        if (Transaction != null) return;

        if (Connection.State == ConnectionState.Closed)
        {
            await Connection.OpenAsync().ConfigureAwait(false);
        }

        Transaction = await Connection.BeginTransactionAsync().ConfigureAwait(false);
    }
}
