using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JoyMoe.Common.Storage.QCloud
{
    public class QCloudWebClient : IDisposable
    {
        private readonly QCloudStorageOptions _options;

        private HttpClient _client;
        private bool _disposed;

        public QCloudWebClient(QCloudStorageOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _client = new HttpClient();

            var version = GetType().Assembly.GetName().Version;

            _client.DefaultRequestHeaders.Add("UserAgent", $"JoyMoe.Common.Storage.QCloud/{version}");
        }

        public void SetHttpClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetAsync(Uri url, Dictionary<string, string>? headers = null, DateTimeOffset? time = null)
            => await SendAsync(url, headers, time).ConfigureAwait(false);

        public async Task<HttpResponseMessage> PostAsync(Uri url, HttpContent content, Dictionary<string, string>? headers = null, DateTimeOffset? time = null)
            => await SendAsync(url, headers, time, HttpMethod.Post, content).ConfigureAwait(false);

        public async Task<HttpResponseMessage> PutAsync(Uri url, HttpContent content, Dictionary<string, string>? headers = null, DateTimeOffset? time = null)
            => await SendAsync(url, headers, time, HttpMethod.Put, content).ConfigureAwait(false);

        public async Task<HttpResponseMessage> DeleteAsync(Uri url, Dictionary<string, string>? headers = null, DateTimeOffset? time = null)
            => await SendAsync(url, headers, time, HttpMethod.Delete).ConfigureAwait(false);

        private async Task<HttpResponseMessage> SendAsync(Uri url, Dictionary<string, string>? headers = null, DateTimeOffset? time = null, HttpMethod? method = null, HttpContent? content = null)
        {
            if (method == null)
            {
                method = HttpMethod.Get;
            }

            using var message = new HttpRequestMessage
            {
                Content = content,
                Method = method,
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

        public Task PrepareRequestAsync(HttpRequestMessage message, bool header = true, DateTimeOffset? time = null)
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
            var keyTime = $"{time.Value.ToUnixTimeSeconds()};{time.Value.AddSeconds(7200).ToUnixTimeSeconds()}";

            var uri = Uri.UnescapeDataString(message.RequestUri!.AbsolutePath);

#pragma warning disable CA1308 // Normalize strings to uppercase
            var parameters = message.RequestUri!
                .ToQueryKeyValuePairs()
                .OrderBy(q => q.Key)
                .Select(q => new KeyValuePair<string, string>(Uri.EscapeDataString(q.Key.ToLowerInvariant()), Uri.EscapeDataString(q.Value)))
                .ToList();
#pragma warning restore CA1308 // Normalize strings to uppercase

            var list = string.Join(";", parameters
                .Select(q => q.Key));
            var query = string.Join("&", parameters
                .Select(q => $"{q.Key}={q.Value}"));

#pragma warning disable CA1308 // Normalize strings to uppercase
            var hd = new SortedDictionary<string, string>();

            foreach (var h in message.Headers)
            {
                hd[Uri.EscapeDataString(h.Key.ToLowerInvariant())] = Uri.EscapeDataString(h.Value.First().Trim());
            }

            if (message.Content?.Headers != null)
            {
                foreach (var h in message.Content.Headers)
                {
                    hd[Uri.EscapeDataString(h.Key.ToLowerInvariant())] = Uri.EscapeDataString(h.Value.First().Trim());
                }
            }
#pragma warning restore CA1308 // Normalize strings to uppercase

            var signed = string.Join(";", hd
                .Select(h => h.Key));
            var headers = string.Join("&", hd
                .Select(h => $"{h.Key}={h.Value}"));

#pragma warning disable CA1308 // Normalize strings to uppercase
            var canonical = $"{message.Method.ToString().ToLowerInvariant()}\n{uri}\n{query}\n{headers}\n";
#pragma warning restore CA1308 // Normalize strings to uppercase

            var hash = canonical.Sha1().ToHex();

            var @string = $"sha1\n{keyTime}\n{hash}\n";

            var signature = CalculateSignature(@string, keyTime);

            if (header)
            {
                var authorization = $"q-sign-algorithm=sha1&q-ak={_options.SecretId}&q-sign-time={keyTime}&q-key-time={keyTime}&q-header-list={signed}&q-url-param-list={list}&q-signature={signature}";
                message.Headers.Remove("Authorization");
                message.Headers.TryAddWithoutValidation("Authorization", authorization);
            }
            else
            {
                message.RequestUri = message.RequestUri.AddQueryParameters(
                    new Dictionary<string, string>
                    {
                        ["q-sign-algorithm"] = "sha1",
                        ["q-ak"] = _options.SecretId,
                        ["q-sign-time"] = keyTime,
                        ["q-key-time"] = keyTime,
                        ["q-header-list"] = signed,
                        ["q-url-param-list"] = list,
                        ["q-signature"] = signature
                    });
            }

            return Task.CompletedTask;
        }

        public string CalculateSignature(string cipher, string keyTime)
        {
            var key = DeriveKeys(keyTime);
            return cipher.HmacSha1(key).ToHex();
        }

        public string DeriveKeys(string keyTime)
        {
            return keyTime.HmacSha1(_options.SecretKey).ToHex();
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
                _client?.Dispose();
            }

            _disposed = true;
        }

    }
}
