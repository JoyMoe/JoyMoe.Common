using System;
using System.Threading;
using System.Threading.Tasks;
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

    public class IdentityDbContextBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>, IDbContextHandler
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            DbContextBase.SetGlobalQueryFilterForSoftDelete(builder);

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
            await DbContextBase.OnBeforeSaving(this).ConfigureAwait(false);
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
