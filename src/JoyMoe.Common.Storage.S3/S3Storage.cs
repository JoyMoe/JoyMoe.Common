using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Storage.S3;

/// <summary>
/// Aws S3 Storage
/// </summary>
public class S3Storage : IObjectStorage
{
    private readonly S3WebClient _client;

    private bool _disposed;

    public S3Storage(IOptions<S3StorageOptions> options) {
        Options = options.Value;

        _client = new S3WebClient(Options);
    }

    public S3StorageOptions Options { get; }

    public async Task<string> DownloadAsync(string path, CancellationToken ct = default) {
        var url      = await GetUrlAsync(path, false, ct);
        var response = await _client.GetAsync(new Uri(url));

        var target = Path.GetTempFileName();

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new IOException();
        }

        await using var file = File.OpenWrite(target);
        await response.Content.CopyToAsync(file);

        return target;
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default) {
        var url = await GetUrlAsync(path, false, ct);
        await _client.DeleteAsync(new Uri(url));
    }

    public async Task UploadAsync(
        string            path,
        Stream            data,
        string            mime,
        bool              everyone = false,
        CancellationToken ct       = default) {
        var url = await GetUrlAsync(path, false, ct);

        using var content = new StreamContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue(mime);

        await _client.PutAsync(new Uri(url),
                               content,
                               new Dictionary<string, string> { ["x-amz-acl"] = everyone ? "public-read" : "private" });
    }

    public async Task<string> GetPublicUrlAsync(string path, CancellationToken ct = default) {
        var url = await GetUrlAsync(path, true, ct);

        using var request = new HttpRequestMessage { RequestUri = new Uri(url) };

        await _client.PrepareRequestAsync(request, false);

        return request.RequestUri.ToString();
    }

    public async Task<ObjectStorageFrontendUploadArguments> GetUploadArgumentsAsync(
        string            path,
        bool              everyone      = false,
        int?              contentLength = null,
        string?           contentType   = null,
        CancellationToken ct            = default) {
        var now       = DateTimeOffset.UtcNow;
        var date      = $"{now:yyyyMMdd}";
        var timestamp = $"{now:yyyyMMddTHHmmssZ}";

        var uri = await GetUrlAsync(string.Empty, true, ct);

        var arguments = new ObjectStorageFrontendUploadArguments
        {
            Action = uri,
            Data =
            {
                ["key"]              = path,
                ["acl"]              = everyone ? "public-read" : "private",
                ["x-amz-algorithm"]  = "AWS4-HMAC-SHA256",
                ["x-amz-credential"] = $"{Options.AccessKey}/{date}/{Options.Region}/s3/aws4_request",
                ["x-amz-date"]       = timestamp
            }
        };

        arguments.Data["policy"] = @$"{{
  ""expiration"": ""{
      now.AddMinutes(30)
      :yyyy-MM-ddTHH:mm:ss.fffZ}"",
  ""conditions"": [
    {{""acl"": ""{
        arguments.Data["acl"]
    }""}},
    {{""bucket"": ""{
        Options.BucketName
    }""}},";

        if (contentLength.HasValue)
        {
            arguments.Data["policy"] += @$"
    [""content-length-range"", 0, {
        contentLength
    }],";
        }

        contentType ??= string.Empty;
        arguments.Data["policy"] += string.IsNullOrWhiteSpace(contentType) || contentType.EndsWith("*")
            ? @$"
[""starts-with"", ""$Content-Type"", ""{
    contentType.TrimEnd('*')
}""],"
            : @$"
{{""Content-Type"": ""{
    contentType
}""}},";

        arguments.Data["policy"] += @$"
    {{""key"": ""{
        arguments.Data["key"]
    }""}},
    {{""x-amz-algorithm"": ""{
        arguments.Data["x-amz-algorithm"]
    }""}},
    {{""x-amz-credential"": ""{
        arguments.Data["x-amz-credential"]
    }""}},
    {{""x-amz-date"": ""{
        arguments.Data["x-amz-date"]
    }""}}
  ]
}}";

        arguments.Data["policy"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(arguments.Data["policy"]));

        arguments.Data["x-amz-signature"] = _client.CalculateSignature(arguments.Data["policy"], date);

        return arguments;
    }

    public Task<string> GetUrlAsync(string path, bool cname = true, CancellationToken ct = default) {
        var protocol = Options.UseHttps ? "https" : "http";

        var prefix = Options.UseCName && cname
            ? $"{protocol}://{Options.BucketName}"
            : $"{protocol}://{Options.Endpoint}/{Options.BucketName}";

        return Task.FromResult($"{prefix}/{path.TrimStart('/')}");
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (_disposed) return;

        if (disposing) _client.Dispose();

        _disposed = true;
    }
}
