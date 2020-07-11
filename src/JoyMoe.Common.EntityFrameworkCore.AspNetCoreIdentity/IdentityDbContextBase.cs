using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JoyMoe.Common.EntityFrameworkCore.AspNetCoreIdentity
{
    public class IdentityDbContextBase : IdentityDbContextBase<IdentityUser, IdentityRole, string>
    {
        public IdentityDbContextBase(DbContextOptions options)
            : base(options)
        {
        }
    }

    public class IdentityDbContextBase<TUser, TRole, TKey> : IdentityDbContextBase<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public IdentityDbContextBase(DbContextOptions options)
            : base(options)
        {
        }
    }

    public class IdentityDbContextBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
        public IdentityDbContextBase(DbContextOptions options)
            : base(options)
        {
        }

        public override int SaveChanges()
        {
            AddTimestampsAsync().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await AddTimestampsAsync().ConfigureAwait(false);
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task AddTimestampsAsync()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is IDataEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (!(entity.Entity is IDataEntity data)) continue;

                var now = DateTimeOffset.Now;

                if (entity.State == EntityState.Added)
                {
                    await OnCreateEntity(data).ConfigureAwait(false);
                    data.CreatedAt = now;
                }

                await OnUpdateEntity(data).ConfigureAwait(false);
                data.UpdatedAt = now;
            }
        }

        protected virtual Task OnCreateEntity(IDataEntity entity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnUpdateEntity(IDataEntity entity)
        {
            return Task.CompletedTask;
        }
    }
}
