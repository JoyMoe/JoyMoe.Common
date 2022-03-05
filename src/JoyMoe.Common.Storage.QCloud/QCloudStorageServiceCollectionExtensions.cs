using System;
using JoyMoe.Common.Storage;
using JoyMoe.Common.Storage.QCloud;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class QCloudStorageServiceCollectionExtensions
{
    /// <summary>
    /// Add a scoped <see cref="QCloudStorage" />.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddQCloudStorage(this IServiceCollection services) {
        services.TryAddScoped<IObjectStorage, QCloudStorage>();

        return services;
    }

    /// <summary>
    /// Add a scoped <see cref="QCloudStorage" />.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddQCloudStorage(
        this IServiceCollection      services,
        Action<QCloudStorageOptions> configure) {
        services.Configure(configure);

        services.AddQCloudStorage();

        return services;
    }
}
