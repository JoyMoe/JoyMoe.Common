using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.EntityFrameworkCore
{
    public class DbContextBase : DbContext, IDbContextHandler
    {
        public DbContextBase(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            SetGlobalQueryFilterForSoftDelete(builder);

            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            OnBeforeSaving().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await OnBeforeSaving().ConfigureAwait(false);
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task OnBeforeSaving()
        {
            await OnBeforeSaving(this).ConfigureAwait(false);
        }

        public static void SetGlobalQueryFilterForSoftDelete(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            foreach (var type in builder.Model.GetEntityTypes())
            {
                if (type == null) continue;
                if (!typeof(ISoftDelete).IsAssignableFrom(type.ClrType)) continue;

                var parameter = Expression.Parameter(type.ClrType, "s");
                var @null = Expression.Constant(null, typeof(object));
                var equal = Expression.Equal(Expression.Property(parameter, nameof(ISoftDelete.DeletedAt)), @null);
                var filter = Expression.Lambda(equal, parameter);
                type.AddQueryFilter(filter);
            }
        }

        public static async Task OnBeforeSaving<T>(T? context) where T : DbContext, IDbContextHandler
        {
            if (context == null)
            {
                return;
            }

            var entries = context.ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                var now = DateTimeOffset.Now;
                var entity = entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                    {
                        await context.OnCreateEntity(entity).ConfigureAwait(false);

                        if (entity is ISoftDelete soft)
                        {
                            soft.DeletedAt = null;
                        }

                        if (entity is ITimestamp data)
                        {
                            data.CreatedAt = now;
                            data.UpdatedAt = now;
                        }

                        break;
                    }
                    case EntityState.Modified:
                    {
                        await context.OnUpdateEntity(entity).ConfigureAwait(false);

                        if (entity is ITimestamp data)
                        {
                            data.UpdatedAt = now;
                        }

                        break;
                    }
                    case EntityState.Deleted:
                    {
                        await context.OnDeleteEntity(entity).ConfigureAwait(false);

                        if (entity is ISoftDelete soft)
                        {
                            entry.State = EntityState.Unchanged;
                            soft.DeletedAt = now;
                        }

                        break;
                    }
                }
            }
        }

        public virtual Task OnCreateEntity(object entity)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDeleteEntity(object entity)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUpdateEntity(object entity)
        {
            return Task.CompletedTask;
        }
    }
}

