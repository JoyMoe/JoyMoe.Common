using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
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

        public override async IAsyncEnumerable<TEntity> ListAsync(
            bool everything = false,
            string? predicate = null,
            [EnumeratorCancellation] CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            var entities = await Connection.ListAsync<TEntity>(predicate, values: values, ct: ct).ConfigureAwait(false);

            foreach (var entity in entities)
            {
                yield return entity;
            }
        }

        public override async Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
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

            predicate = FilteringQuery(predicate, everything);

            predicate += $" ORDER BY @{key} DESC ";
            predicate += $" LIMIT {size} ";

            return await Connection.ListAsync<TEntity>(predicate, values: values, ct: ct).ConfigureAwait(false);
        }

        public override async Task<TEntity?> FirstOrDefaultAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            return await Connection.FirstOrDefaultAsync<TEntity>(predicate, values: values, ct: ct).ConfigureAwait(false);
        }

        public override async Task<TEntity?> SingleOrDefaultAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            return await Connection.SingleOrDefaultAsync<TEntity>(predicate, values: values, ct: ct).ConfigureAwait(false);
        }

        public override async Task<bool> AnyAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            var count = await CountAsync(everything, predicate, values: values, ct: ct).ConfigureAwait(false);

            return count > 0;
        }

        public override async Task<long> CountAsync(
            bool everything = false,
            string? predicate = null,
            CancellationToken ct = default,
            params object[] values)
        {
            predicate = FilteringQuery(predicate, everything);

            return await Connection.CountAsync<TEntity>(predicate, values: values, ct: ct).ConfigureAwait(false);
        }

        public override async Task AddAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await BeginTransactionAsync(ct).ConfigureAwait(false);

            await OnBeforeAddAsync(entity, ct).ConfigureAwait(false);

            await Connection.InsertAsync(entity, Transaction, ct).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await BeginTransactionAsync(ct).ConfigureAwait(false);

            await OnBeforeUpdateAsync(entity, ct).ConfigureAwait(false);

            await Connection.UpdateAsync(entity, Transaction, ct).ConfigureAwait(false);
        }

        public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await BeginTransactionAsync(ct).ConfigureAwait(false);

            if (await OnBeforeRemoveAsync(entity, ct).ConfigureAwait(false))
            {
                await Connection.DeleteAsync(entity, Transaction, ct).ConfigureAwait(false);
                return;
            }

            await Connection.UpdateAsync(entity, Transaction, ct).ConfigureAwait(false);
        }

        public override async Task<int> CommitAsync(CancellationToken ct = default)
        {
            if (Transaction == null) return 0;

            await Transaction.CommitAsync(ct).ConfigureAwait(false);

            Transaction = null;

            // TODO: Get count of changes

            return 1;
        }

        private async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (Transaction != null) return;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(ct).ConfigureAwait(false);
            }

            Transaction = await Connection.BeginTransactionAsync(ct).ConfigureAwait(false);
        }

        private static string? FilteringQuery(string? predicate, bool everything)
        {
            if (everything)
            {
                return predicate;
            }

            if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return predicate;
            }

            var filtering = $"@{nameof(ISoftDelete.DeletedAt)} IS NULL";

            return string.IsNullOrEmpty(predicate)
                ? filtering
                : $"( {predicate} ) AND {filtering}";
        }
    }
}
