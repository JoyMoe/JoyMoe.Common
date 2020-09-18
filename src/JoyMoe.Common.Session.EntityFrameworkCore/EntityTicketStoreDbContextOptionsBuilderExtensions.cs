using System;
using JoyMoe.Common.Session.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class EntityTicketStoreDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Configure the Session Storage Model
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder UseSessionStore<TUser>(this DbContextOptionsBuilder builder)
            where TUser : class
        {
            return builder.UseSessionStore<TUser, EntityTicketStoreSession<TUser>>();
        }

        /// <summary>
        /// Configure the Session Storage Model
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder UseSessionStore<TUser, TSession>(this DbContextOptionsBuilder builder)
            where TSession : EntityTicketStoreSession<TUser>
            where TUser : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.ReplaceService<IModelCustomizer, EntityTicketStoreEntityFrameworkCoreCustomizer<TUser, TSession>>();
        }
    }
}
