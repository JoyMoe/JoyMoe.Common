using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JoyMoe.Common.Oss.S3
{
    public static class S3StorageServiceCollectionExtensions
    {
        /// <summary>
        /// Add a scoped <see cref="S3Storage"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddS3Storage(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddScoped<IOssStorage, S3Storage>();

            return services;
        }

        /// <summary>
        /// Add a scoped <see cref="S3Storage"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddS3Storage(
            this IServiceCollection services,
            Action<S3StorageOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);

            services.AddS3Storage();

            return services;
        }
    }
}
