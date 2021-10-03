using System;
using JoyMoe.Common.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DiagnosticsMvcBuilderExtensions
{
    public static IServiceCollection AddApiProblemDetailsFactory(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<ProblemDetailsFactory, ApiProblemDetailsFactory>();

        return services;
    }

    public static IHealthChecksBuilder AddRfcHealthChecks(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<IPostConfigureOptions<HealthCheckOptions>, RfcHealthCheckWriterPostConfigureOptions>();

        return services.AddHealthChecks();
    }
}
