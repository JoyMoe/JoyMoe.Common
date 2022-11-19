using System.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace System.Net.Http;

public static class HttpHeadersExtensions
{
    private static string AppendParameter(string query, KeyValuePair<string, string> pair) {
        var kv = string.IsNullOrWhiteSpace(pair.Value)
            ? $"{Uri.EscapeDataString(pair.Key)}"
            : $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}";

        if (query.Length == 0) {
            query = kv;
        } else {
            query += $"&{kv}";
        }

        return query;
    }

    public static Uri AddQueryParameter(this Uri uri, string key, string value) {
        var url   = uri.GetLeftPart(UriPartial.Path);
        var query = uri.Query.TrimStart('?');

        query = AppendParameter(query, new KeyValuePair<string, string>(key, value));

        return new Uri($"{url}?{query}", uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }

    public static Uri AddQueryParameters(this Uri uri, IEnumerable<KeyValuePair<string, string>> pairs) {
        var url   = uri.GetLeftPart(UriPartial.Path);
        var query = uri.Query.TrimStart('?');

        query = pairs.Aggregate(query, AppendParameter);

        return new Uri($"{url}?{query}", uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }

    public static string? FindFirstValue(this HttpHeaders headers, string key) {
        return headers.FirstOrDefault(h => h.Key == key).Value?.FirstOrDefault();
    }

    public static IEnumerable<KeyValuePair<string, string>> ToQueryKeyValuePairs(this Uri uri) {
        return uri.Query.TrimStart('?').Split('&').Where(s => s.Trim().Length > 0).Select(s => s.Split('=')).Select(
            sp => sp.Length == 2
                ? new KeyValuePair<string, string>(Uri.UnescapeDataString(sp[0]), Uri.UnescapeDataString(sp[1]))
                : new KeyValuePair<string, string>(Uri.UnescapeDataString(sp[0]), ""));
    }
}
