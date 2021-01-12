using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        bool IgnoreQueryFilters { get; set; }

        Task<TEntity?> FindAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey id,
            CancellationToken ct = default)
            where TKey : struct;

        IAsyncEnumerable<TEntity> FindAllAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            IEnumerable<TKey> ids,
            CancellationToken ct = default)
            where TKey : struct;

        IAsyncEnumerable<TEntity> ListAsync(
            Expression<Func<TEntity, bool>>? predicate,
            CancellationToken ct = default);

        IAsyncEnumerable<TEntity> ListAsync<TKey>(
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TKey>>? ordering,
            int? limitation,
            CancellationToken ct = default)
            where TKey : struct;

        Task<IEnumerable<TEntity>> PaginateAsync<TKey>(
            Expression<Func<TEntity, TKey>> selector,
            TKey? before = null,
            int size = 10,
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken ct = default)
            where TKey : struct, IComparable;

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);
        Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);
        Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct = default);

        Task AddAsync(TEntity entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

        Task UpdateAsync(TEntity entity, CancellationToken ct = default);

        Task RemoveAsync(TEntity entity, CancellationToken ct = default);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

        Task<int> CommitAsync(CancellationToken ct = default);
    }
}
