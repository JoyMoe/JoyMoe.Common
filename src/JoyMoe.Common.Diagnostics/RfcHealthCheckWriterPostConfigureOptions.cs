using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Diagnostics;

public class RfcHealthCheckWriterPostConfigureOptions : IPostConfigureOptions<HealthCheckOptions>
{
    public void PostConfigure(string name, HealthCheckOptions options)
    {
        options.ResponseWriter = RfcHealthCheckWriter.WriteResponse;
    }
}
