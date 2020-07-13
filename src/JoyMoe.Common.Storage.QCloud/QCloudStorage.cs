using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Storage.QCloud
{
    /// <summary>
    /// Tencent Cloud Object Storage
    /// </summary>
    public class QCloudStorage : IObjectStorage
    {
        private readonly QCloudWebClient _client;

        private bool _disposed;

        public QCloudStorage(IOptions<QCloudStorageOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            Options = optionsAccessor.Value;

            _client = new QCloudWebClient(Options);
        }

        public QCloudStorageOptions Options { get; }

        public async Task WriteStreamAsync(string path, Stream data, string mime, bool everyone = false, CancellationToken ct = default)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var url = await GetUrlAsync(path, false, ct).ConfigureAwait(false);

            using var content = new StreamContent(data);
            content.Headers.ContentLength = data.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(mime);
            content.Headers.ContentMD5 = data.Md5();
            data.Seek(0, SeekOrigin.Begin);

            await _client.PutAsync(new Uri(url), content, new Dictionary<string, string>
            {
                ["x-cos-acl"] = everyone ? "public-read" : "private"
            }).ConfigureAwait(false);
        }

        public async Task<ObjectStorageFrontendUploadArguments> GetUploadArgumentsAsync(string path, bool everyone = false, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var expiration = now.AddSeconds(1800);
            var keyTime = $"{now.ToUnixTimeSeconds()};{expiration.ToUnixTimeSeconds()}";

            var uri = await GetUrlAsync(path, true, ct).ConfigureAwait(false);

            var arguments = new ObjectStorageFrontendUploadArguments
            {
                Action = uri,
                Data =
                {
                    ["key"] = path,
                    ["acl"] = everyone ? "public-read" : "private",
                    ["q-sign-algorithm"] = "sha1",
                    ["q-ak"] = Options.SecretId,
                    ["q-key-time"] = keyTime,
                }
            };

            arguments.Data["policy"] = @$"{{
  ""expiration"": ""{expiration:yyyyMMddTHHmmssZ}"",
  ""conditions"": [
    {{""acl"": ""{arguments.Data["acl"]}""}},
    {{""bucket"": ""{Options.BucketName}""}},
    [""content-length-range"", 4096, 1048576],
    [""starts-with"", ""$Content-Type"", ""image/""],
    {{""key"": ""{arguments.Data["key"]}""}},
    {{""q-ak"": ""{arguments.Data["q-ak"]}""}},
    {{""q-sign-algorithm"": ""{arguments.Data["q-sign-algorithm"]}""}},
    {{""q-sign-time"": ""{arguments.Data["q-key-time"]}""}}
  ]
}}";

            arguments.Data["q-signature"] = _client.CalculateSignature(arguments.Data["policy"], keyTime);
            arguments.Data["policy"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(arguments.Data["q-signature"]));

            return arguments;
        }

        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            var url = await GetUrlAsync(path, false, ct).ConfigureAwait(false);
            await _client.DeleteAsync(new Uri(url)).ConfigureAwait(false);
        }

        public Task<string> GetUrlAsync(string path, bool cname = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var protocol = Options.UseHttps ? "https" : "http";

            var prefix = !string.IsNullOrWhiteSpace(Options.CanonicalName) && cname
                ? $"{protocol}://{Options.CanonicalName}"
                : $"{protocol}://{Options.Endpoint}";

            return Task.FromResult($"{prefix}/{path.TrimStart('/')}");
        }

        public async Task<string> GetPublicUrlAsync(string path, CancellationToken ct = default)
        {
            var url = await GetUrlAsync(path, true, ct).ConfigureAwait(false);

            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url)
            };

            await _client.PrepareRequestAsync(request, false).ConfigureAwait(false);

            return request.RequestUri.ToString();
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
