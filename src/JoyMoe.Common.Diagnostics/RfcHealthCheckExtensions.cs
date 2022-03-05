using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Diagnostics.HealthChecks;

public static class RfcHealthCheckExtensions
{
    public static string ToRfcStatusString(this HealthStatus status) {
        return status switch
        {
            HealthStatus.Unhealthy => "fail",
            HealthStatus.Degraded  => "warn",
            HealthStatus.Healthy   => "pass",
            _                      => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
