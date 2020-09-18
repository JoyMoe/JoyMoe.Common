using System;
using JoyMoe.Common.Session;
using JoyMoe.Common.Session.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class EntityTicketStoreServiceCollectionExtensions
    {
        /// <summary>
        /// Add a <see cref="EntityTicketStore{TContext, TUser, EntityTicketStoreSession}"/> to preserve identity information
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEntityTicketStore<TContext, TUser>(this IServiceCollection services)
            where TContext : DbContext
            where TUser : class
        {
            return services.AddEntityTicketStore<TContext, TUser, EntityTicketStoreSession<TUser>>();
        }

        /// <summary>
        /// Add a <see cref="EntityTicketStore{TContext, TUser, TSession}"/> to preserve identity information
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEntityTicketStore<TContext, TUser, TSession>(this IServiceCollection services)
            where TContext : DbContext
            where TSession : EntityTicketStoreSession<TUser>
            where TUser : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ITicketStore, EntityTicketStore<TContext, TUser, TSession>>();
            services.TryAddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, SessionStoreOptions>();

            return services;
        }
    }
}
