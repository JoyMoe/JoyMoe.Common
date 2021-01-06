using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data.Dapper
{
    public class DapperRepository<TEntity> : RepositoryBase<TEntity> where TEntity : class
    {
        protected DbConnection Connection { get; }
        protected DbTransaction? Transaction { get; set; }

        public DapperRepository(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public override async IAsyncEnumerable<TEntity> ListAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            var entities = await Connection.ListAsync<TEntity>(predicate, values).ConfigureAwait(false);

            foreach (var entity in entities)
            {
                yield return entity;
            }
        }

        public override async Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            string? predicate = null,
            params object[] values)
        {
            var key = selector.GetColumnName();

            if (before != null)
            {
                predicate = string.IsNullOrEmpty(predicate)
                    ? $"@{key} < {{{values.Length}}}"
                    : $"( {predicate} ) AND @{key} < {{{values.Length}}}";

                Array.Resize(ref values, values.Length + 1);
                values[values.GetUpperBound(0)] = before;
            }

            predicate = FilteringQuery(predicate);

            predicate += $" ORDER BY @{key} DESC ";
            predicate += $" LIMIT {size} ";

            return await Connection.ListAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async Task<TEntity?> FirstOrDefaultAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return await Connection.FirstOrDefaultAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async Task<TEntity?> SingleOrDefaultAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return await Connection.SingleOrDefaultAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async Task<bool> AnyAsync(string? predicate = null, params object[] values)
        {
            var count = await CountAsync(predicate, values).ConfigureAwait(false);

            return count > 0;
        }

        public override async Task<long> CountAsync(string? predicate = null, params object[] values)
        {
            predicate = FilteringQuery(predicate);

            return await Connection.CountAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async Task AddAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await BeginTransactionAsync().ConfigureAwait(false);

            await OnBeforeAddAsync(entity).ConfigureAwait(false);

            await Connection.InsertAsync(entity, Transaction).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await BeginTransactionAsync().ConfigureAwait(false);

            await OnBeforeUpdateAsync(entity).ConfigureAwait(false);

            await Connection.UpdateAsync(entity, Transaction).ConfigureAwait(false);
        }

        public override async Task RemoveAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await BeginTransactionAsync().ConfigureAwait(false);

            if (await OnBeforeRemoveAsync(entity).ConfigureAwait(false))
            {
                await Connection.DeleteAsync(entity, Transaction).ConfigureAwait(false);
                return;
            }

            await Connection.UpdateAsync(entity, Transaction).ConfigureAwait(false);
        }

        public override async Task<int> CommitAsync()
        {
            if (Transaction == null) return 0;

            await Transaction.CommitAsync().ConfigureAwait(false);

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
}
