using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Storage.S3
{
    /// <summary>
    /// Aws S3 Storage
    /// </summary>
    public class S3Storage : IObjectStorage
    {
        private readonly S3WebClient _client;

        private bool _disposed = false;

        public S3Storage(IOptions<S3StorageOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            Options = optionsAccessor.Value;

            _client = new S3WebClient(Options);
        }

        public S3StorageOptions Options { get; }

        public async Task WriteStreamAsync(string path, Stream data, string mime, bool everyone = false, CancellationToken ct = default)
        {
            var url = await GetUrlAsync(path, false, ct).ConfigureAwait(false);
            using var content = new StreamContent(data);
            await _client.PutAsync(new Uri(url), content, new Dictionary<string, string>
            {
                ["content-type"] = mime,
                ["x-amz-acl"] = everyone ? "public-read" : "private"
            }).ConfigureAwait(false);
        }

        public async Task<ObjectStorageFrontendUploadArguments> GetUploadArgumentsAsync(string path, bool everyone = false, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var date = $"{now:yyyyMMdd}";

            var uri = await GetUrlAsync(path, true, ct).ConfigureAwait(false);

            var arguments = new ObjectStorageFrontendUploadArguments
            {
                Action = uri,
                Data =
                {
                    ["key"] = path,
                    ["acl"] = everyone ? "public-read" : "private",
                    ["x-amz-algorithm"] = "AWS4-HMAC-SHA256",
                    ["x-amz-credential"] = $"{Options.AccessKey}/{date}/{Options.Region}/s3/aws4_request",
                    ["x-amz-date"] = date,
                }
            };

            arguments.Data["policy"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(@$"{{
  ""expiration"": ""{now.AddMinutes(30):yyyyMMddTHHmmssZ}"",
  ""conditions"": [
    {{""bucket"": ""{Options.BucketName}""}},
    {{""key"": ""{arguments.Data["key"]}""}},
    {{""acl"": ""{arguments.Data["acl"]}""}},
    [""content-length-range"", 4096, 1048576],
    [""starts-with"", ""$Content-Type"", ""image/""],
    {{""x-amz-algorithm"": ""{arguments.Data["x-amz-algorithm"]}""}},
    {{""x-amz-credential"": ""{arguments.Data["x-amz-credential"]}""}},
    {{""x-amz-date"": ""{arguments.Data["x-amz-date"]}""}}
  ]
}}"));

            arguments.Data["x-amz-signature"] = _client.CalculateSignature(arguments.Data["policy"], date);

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

            var prefix = Options.UseCName && cname
                ? $"{protocol}://{Options.BucketName}"
                : $"{protocol}://{Options.Endpoint}//{Options.BucketName}";

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
