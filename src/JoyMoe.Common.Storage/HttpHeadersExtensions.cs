using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Net.Http.Headers
{
    public static class HttpHeadersExtensions
    {
        public static string? FindFirstValue(this HttpHeaders headers, string key)
        {
            return headers.FirstOrDefault(h => h.Key == key).Value?.FirstOrDefault();
        }
    }
}
