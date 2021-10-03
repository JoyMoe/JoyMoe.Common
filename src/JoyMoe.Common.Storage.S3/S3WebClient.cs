using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace JoyMoe.Common.Storage.S3;

public class S3WebClient : IDisposable
{
    private readonly S3StorageOptions _options;

    private HttpClient _client = new();

    private bool _disposed;

    public S3WebClient(S3StorageOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        var version = GetType().Assembly.GetName().Version;

        _client.DefaultRequestHeaders.Add("UserAgent", $"JoyMoe.Common.Storage.S3/{version}");
    }

    public void SetHttpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> GetAsync(Uri             url, Dictionary<string, string>? headers = null,
                                                    DateTimeOffset? time = null)
    {
        return await SendAsync(url, headers, time).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostAsync(Uri                         url, HttpContent content,
                                                     Dictionary<string, string>? headers = null,
                                                     DateTimeOffset?             time    = null)
    {
        return await SendAsync(url, headers, time, HttpMethod.Post, content).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PutAsync(Uri                         url, HttpContent content,
                                                    Dictionary<string, string>? headers = null,
                                                    DateTimeOffset?             time    = null)
    {
        return await SendAsync(url, headers, time, HttpMethod.Put, content).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> DeleteAsync(Uri             url, Dictionary<string, string>? headers = null,
                                                       DateTimeOffset? time = null)
    {
        return await SendAsync(url, headers, time, HttpMethod.Delete).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendAsync(Uri             url, Dictionary<string, string>? headers = null,
                                                      DateTimeOffset? time    = null, HttpMethod? method = null,
                                                      HttpContent?    content = null)
    {
        method ??= HttpMethod.Get;

        using var message = new HttpRequestMessage
        {
            Content    = content,
            Method     = method,
            RequestUri = url
        };

        if (headers != null)
        {
            foreach (var h in headers)
            {
                message.Headers.Add(h.Key, h.Value);
            }
        }

        await PrepareRequestAsync(message, true, time).ConfigureAwait(false);

        return await _client.SendAsync(message).ConfigureAwait(false);
    }

    public async Task PrepareRequestAsync(HttpRequestMessage message, bool header = true, DateTimeOffset? time = null)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (message.RequestUri == null)
        {
            throw new NullReferenceException();
        }

        message.Headers.Host = message.RequestUri.Host;

        time ??= DateTimeOffset.UtcNow;
        var timestamp = $"{time:yyyyMMddTHHmmssZ}";
        var date      = $"{time:yyyyMMdd}";

        const string algorithm = "AWS4-HMAC-SHA256";

        var hash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        if (!header)
        {
            hash = "UNSIGNED-PAYLOAD";
        }
        else if (message.Content != null)
        {
            var payload = await message.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            hash = payload.Sha256().ToHex();
        }

        var scope      = $"{date}/{_options.Region}/s3/aws4_request";
        var credential = $"{_options.AccessKey}/{scope}";

        if (header)
        {
            message.Headers.Add("x-amz-date", timestamp);
            message.Headers.Add("x-amz-content-sha256", hash);
        }

#pragma warning disable CA1308 // Normalize strings to uppercase
        var hd = new SortedDictionary<string, string>();

        foreach (var h in message.Headers)
        {
            hd[h.Key.ToLowerInvariant()] = h.Value.First().Trim();
        }

        if (message.Content?.Headers != null)
        {
            foreach (var h in message.Content.Headers)
            {
                hd[h.Key.ToLowerInvariant()] = h.Value.First().Trim();
            }
        }
#pragma warning restore CA1308 // Normalize strings to uppercase

        var signed = string.Join(";", hd
                                    .Select(h => h.Key));
        var headers = string.Join("", hd
                                     .Select(h => $"{h.Key}:{h.Value}\n"));

        if (!header)
        {
            message.RequestUri = message.RequestUri.AddQueryParameters(
                new Dictionary<string, string>
                {
                    ["X-Amz-Algorithm"]     = algorithm,
                    ["X-Amz-Credential"]    = credential,
                    ["X-Amz-Date"]          = timestamp,
                    ["X-Amz-Expires"]       = "86400",
                    ["X-Amz-SignedHeaders"] = signed
                });
        }

        var uri = Uri.EscapeDataString(message.RequestUri!.AbsolutePath)
                     .Replace("%2F", "/");
        var query = string.Join("&", message.RequestUri!
                                            .ToQueryKeyValuePairs()
                                            .OrderBy(q => q.Key)
                                            .Select(
                                                 q =>
                                                     $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value)}"));

        var canonical = $"{message.Method}\n{uri}\n{query}\n{headers}\n{signed}\n{hash}";

        var request = canonical.Sha256().ToHex();

        var @string = $"AWS4-HMAC-SHA256\n{timestamp}\n{scope}\n{request}";

        var signature = CalculateSignature(@string, date);

        if (header)
        {
            var authorization =
                $"Credential={credential},SignedHeaders={string.Join(";", signed)},Signature={signature}";
            message.Headers.Authorization = new AuthenticationHeaderValue("AWS4-HMAC-SHA256", authorization);
        }
        else
        {
            message.RequestUri = message.RequestUri.AddQueryParameter("X-Amz-Signature", signature);
        }
    }

    public string CalculateSignature(string cipher, string date)
    {
        var key = DeriveKeys(date);
        return cipher.HmacSha256(key).ToHex();
    }

    public byte[] DeriveKeys(string date)
    {
        var dateKey    = date.HmacSha256($"AWS4{_options.SecretKey}");
        var regionKey  = _options.Region.HmacSha256(dateKey);
        var serviceKey = "s3".HmacSha256(regionKey);
        return "aws4_request".HmacSha256(serviceKey);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _client.Dispose();
        }

        _disposed = true;
    }
}
