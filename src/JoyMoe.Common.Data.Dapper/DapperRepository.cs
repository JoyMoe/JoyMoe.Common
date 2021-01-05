using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
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

        public override async IAsyncEnumerable<TEntity> ListAsync(string? predicate, List<object>? values, bool everything = false)
        {
            predicate = FilteringQuery(predicate, everything);

            var entities = await Connection.ListAsync<TEntity>(predicate, values).ConfigureAwait(false);

            foreach (var entity in entities)
            {
                yield return entity;
            }
        }

        public override async ValueTask<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            string? predicate = null,
            List<object>? values = null,
            bool everything = false,
            CancellationToken ct = default)
        {
            values ??= new List<object>();

            var key = selector.GetColumnName();

            if (before != null)
            {
                predicate = string.IsNullOrEmpty(predicate)
                    ? $"@{key} < {{{values.Count}}}"
                    : $"( {predicate} ) AND @{key} < {{{values.Count}}}";
                values.Add(before);
            }

            predicate = FilteringQuery(predicate, everything);

            predicate += $" ORDER BY @{key} DESC ";
            predicate += $" LIMIT {size} ";

            return await Connection.ListAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async ValueTask<TEntity?> FirstOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await Connection.FirstOrDefaultAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async ValueTask<TEntity?> SingleOrDefaultAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await Connection.SingleOrDefaultAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async ValueTask<bool> AnyAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            var count = await CountAsync(predicate, values, everything, ct).ConfigureAwait(false);

            return count > 0;
        }

        public override async ValueTask<long> CountAsync(string? predicate, List<object>? values, bool everything = false, CancellationToken ct = default)
        {
            predicate = FilteringQuery(predicate, everything);

            return await Connection.CountAsync<TEntity>(predicate, values).ConfigureAwait(false);
        }

        public override async Task AddAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Transaction == null)
            {
                if (Connection.State == ConnectionState.Closed) await Connection.OpenAsync(ct).ConfigureAwait(false);
                Transaction = await Connection.BeginTransactionAsync(ct).ConfigureAwait(false);
            }

            var now = DateTime.UtcNow;

            if (entity is ITimestamp stamp)
            {
                stamp.CreatedAt = now;
                stamp.UpdatedAt = now;
            }

            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = null;
            }

            await Connection.InsertAsync(entity, Transaction).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Transaction == null)
            {
                if (Connection.State == ConnectionState.Closed) await Connection.OpenAsync(ct).ConfigureAwait(false);
                Transaction = await Connection.BeginTransactionAsync(ct).ConfigureAwait(false);
            }

            if (entity is ITimestamp stamp)
            {
                stamp.UpdatedAt = DateTime.UtcNow;
            }

            await Connection.UpdateAsync(entity, Transaction).ConfigureAwait(false);
        }

        public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Transaction == null)
            {
                if (Connection.State == ConnectionState.Closed) await Connection.OpenAsync(ct).ConfigureAwait(false);
                Transaction = await Connection.BeginTransactionAsync(ct).ConfigureAwait(false);
            }

            if (entity is ISoftDelete soft)
            {
                soft.DeletedAt = DateTime.UtcNow;

                await Connection.UpdateAsync(entity, Transaction).ConfigureAwait(false);

                return;
            }

            await Connection.DeleteAsync(entity, Transaction).ConfigureAwait(false);
        }

        public override async ValueTask<int> CommitAsync(CancellationToken ct = default)
        {
            if (Transaction == null) return 0;

            await Transaction.CommitAsync(ct).ConfigureAwait(false);

            await Transaction.DisposeAsync().ConfigureAwait(false);

            Transaction = null;

            // TODO: Get count of changes

            return 1;
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
