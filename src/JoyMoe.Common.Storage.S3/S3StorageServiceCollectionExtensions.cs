using JoyMoe.Common.Storage;
using JoyMoe.Common.Storage.S3;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class S3StorageServiceCollectionExtensions
{
    /// <summary>
    /// Add a scoped <see cref="S3Storage" />.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddS3Storage(this IServiceCollection services) {
        services.TryAddScoped<IObjectStorage, S3Storage>();

        return services;
    }

    /// <summary>
    /// Add a scoped <see cref="S3Storage" />.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddS3Storage(
        this IServiceCollection  services,
        Action<S3StorageOptions> configure) {
        services.Configure(configure);

        services.AddS3Storage();

        return services;
    }
}
