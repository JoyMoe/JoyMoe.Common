using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Session
{
    public static class CachedTicketStoreServiceCollectionExtensions
    {
        /// <summary>
        /// Add a <see cref="CachedTicketStore"/> to preserve identity information
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCachedTicketStore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ITicketStore, CachedTicketStore>();
            services.TryAddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, SessionStoreOptions>();

            return services;
        }
    }
}
