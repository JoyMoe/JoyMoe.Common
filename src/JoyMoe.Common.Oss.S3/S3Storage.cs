using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Oss.S3
{
    /// <summary>
    /// Aws S3 Storage
    /// </summary>
    public class S3Storage : IOssStorage, IDisposable
    {
        private bool _disposed = false;

        private readonly RegionEndpoint _endpoint;
        private readonly AmazonS3Client _s3Client;

        private readonly string _protocol;

        public S3Storage(IOptions<S3StorageOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            Options = optionsAccessor.Value;

            _endpoint = RegionEndpoint.GetBySystemName(Options.Region);

            _s3Client = new AmazonS3Client(Options.AccessKey, Options.SecretKey, new AmazonS3Config
            {
                RegionEndpoint = _endpoint,
                UseHttp = Options.UseHttps
            });

            _protocol = Options.UseHttps ? "https" : "http";
        }

        public S3StorageOptions Options { get; }

        public async Task WriteStreamAsync(string path, Stream data, string mime, bool everyone = false, CancellationToken ct = default)
        {
            await ((IAmazonS3)_s3Client).UploadObjectFromStreamAsync(Options.BucketName, path, data, new Dictionary<string, object>
            {
                ["ContentType"] = mime,
                ["CannedACL"] = everyone ? S3CannedACL.PublicRead : S3CannedACL.Private
            }, ct).ConfigureAwait(false);
        }

        public Task<Dictionary<string, string>> GetUploadFormAsync(string path, bool everyone = false, CancellationToken ct = default)
        {
            var date = DateTime.Now.ToUniversalTime();

            var vm = new Dictionary<string, string>
            {
                ["bucket"] = Options.BucketName,
                ["key"] = path,
                ["acl"] = everyone ? "public-read" : "private",
                ["x-amz-algorithm"] = "AWS4-HMAC-SHA256",
                ["x-amz-credential"] = $"{Options.AccessKey}/{date:yyyyMMdd}/{Options.Region}/s3/aws4_request",
                ["x-amz-date"] = $"{date:yyyyMMddTHHmmssZ}",
            };

            vm["policy"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(@$"{{
  ""expiration"": ""{date.AddMinutes(30):yyyy-MM-ddTHH:mm:ss.fffZ}"",
  ""conditions"": [
    {{""bucket"": ""{vm["bucket"]}""}},
    {{""key"": ""{vm["key"]}""}},
    {{""acl"": ""{vm["acl"]}""}},
    [""content-length-range"", 4096, 1048576],
    [""starts-with"", ""$Content-Type"", ""image/""],
    {{""x-amz-algorithm"": ""{vm["x-amz-algorithm"]}""}},
    {{""x-amz-credential"": ""{vm["x-amz-credential"]}""}},
    {{""x-amz-date"": ""{vm["x-amz-date"]}""}}
  ]
}}"));

            var key = AWS4Signer.ComposeSigningKey(Options.SecretKey, Options.Region, $"{date:yyyyMMdd}", "s3");
            var signature = AWS4Signer.ComputeKeyedHash(SigningAlgorithm.HmacSHA256, key, vm["policy"]);

#pragma warning disable CA1308 // Normalize strings to uppercase
            vm["x-amz-signature"] = BitConverter.ToString(signature).Replace("-", string.Empty, StringComparison.InvariantCulture).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            return Task.FromResult(vm);
        }

        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            await _s3Client.DeleteObjectAsync(Options.BucketName, path, ct).ConfigureAwait(false);
        }

        public Task<string> GetUrlAsync(string path, CancellationToken ct = default)
        {
            return Task.FromResult(Options.UseCname
                ? $"{_protocol}://{Options.BucketName}/{path}"
                : $"{_protocol}://{Options.BucketName}.{_endpoint.GetEndpointForService("s3")}/{path}");
        }

        public Task<string> GetPublicUrlAsync(string path, CancellationToken ct = default)
        {
            var expiration = DateTimeOffset.Now.AddSeconds(3600);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = Options.BucketName,
                Key = path,
                Expires = expiration.UtcDateTime,
                Protocol = Options.UseHttps ? Protocol.HTTPS : Protocol.HTTP
            };

            if (Options.UseCname)
            {
                request.Headers["Host"] = Options.BucketName;
            }

            var url = _s3Client.GetPreSignedURL(request);

            if (Options.UseCname)
            {
                url = url.Replace($"{Options.BucketName}.{_endpoint.GetEndpointForService("s3")}", Options.BucketName, StringComparison.InvariantCulture);
            }

            return Task.FromResult(url);
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
                _s3Client?.Dispose();
            }

            _disposed = true;
        }
    }
}
