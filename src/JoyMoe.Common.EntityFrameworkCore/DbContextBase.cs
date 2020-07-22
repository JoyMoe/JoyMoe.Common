using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.EntityFrameworkCore
{
    public class DbContextBase : DbContext
    {
        public DbContextBase(DbContextOptions options)
            : base(options)
        {
        }

        public override int SaveChanges()
        {
            AddTimestampsAsync().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await AddTimestampsAsync().ConfigureAwait(false);
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task AddTimestampsAsync()
        {
            var entries = ChangeTracker.Entries()
                .Where(x => x.Entity is IDataEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var now = DateTimeOffset.Now;
                var entity = entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    await OnCreateEntity(entity).ConfigureAwait(false);

                    if (entity is IDataEntity data)
                    {
                        data.CreatedAt = now;
                        data.UpdatedAt = now;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    await OnUpdateEntity(entity).ConfigureAwait(false);

                    if (entity is IDataEntity data)
                    {
                        data.UpdatedAt = now;
                    }
                }
            }
        }

        protected virtual Task OnCreateEntity(object entity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnUpdateEntity(object entity)
        {
            return Task.CompletedTask;
        }
    }
}
