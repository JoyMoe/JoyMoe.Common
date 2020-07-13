using System.Linq;
using System.Net.Http.Headers;

namespace JoyMoe.Common.Storage.S3.Tests
{
    public static class HttpHeadersExtensions
    {
        public static string? FindFirstValue(this HttpHeaders headers, string key)
        {
            return headers.FirstOrDefault(h => h.Key == key).Value?.FirstOrDefault();
        }
    }
}