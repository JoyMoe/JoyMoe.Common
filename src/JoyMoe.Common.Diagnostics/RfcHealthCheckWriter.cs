using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JoyMoe.Common.Diagnostics;

public static class RfcHealthCheckWriter
{
    public static async Task WriteResponse(HttpContext context, HealthReport result) {
        context.Response.ContentType = "application/json";

        var options = new JsonWriterOptions { Indented = true };

        await using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream, options);

        writer.WriteStartObject();

        writer.WriteString("status", result.Status.ToRfcStatusString());

        writer.WriteStartObject("details");
        foreach (var (key, entry) in result.Entries)
        {
            writer.WriteStartObject(key);

            writer.WriteString("status", entry.Status.ToRfcStatusString());
            writer.WriteString("description", entry.Description);

            writer.WriteStartObject("data");
            foreach (var (s, value) in entry.Data)
            {
                writer.WritePropertyName(s);
                JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object));
            }

            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        writer.WriteEndObject();

        writer.WriteEndObject();

        await writer.FlushAsync();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        await context.Response.WriteAsync(json);
    }
}
