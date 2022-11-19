using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Storage.QCloud;

/// <summary>
/// Tencent Cloud Object Storage
/// </summary>
public class QCloudStorage : IObjectStorage
{
    private readonly QCloudWebClient _client;

    private bool _disposed;

    public QCloudStorage(IOptions<QCloudStorageOptions> optionsAccessor) {
        Options = optionsAccessor.Value;

        _client = new QCloudWebClient(Options);
    }

    public QCloudStorageOptions Options { get; }

    public async Task<string> DownloadAsync(string path, CancellationToken ct = default) {
        var url      = await GetUrlAsync(path, false, ct);
        var response = await _client.GetAsync(new Uri(url));

        var target = Path.GetTempFileName();

        if (string.IsNullOrWhiteSpace(target)) {
            throw new IOException();
        }

        using var file = File.OpenWrite(target);
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
        content.Headers.ContentLength = data.Length;
        content.Headers.ContentType   = new MediaTypeHeaderValue(mime);
        content.Headers.ContentMD5    = data.Md5();
        data.Seek(0, SeekOrigin.Begin);

        await _client.PutAsync(new Uri(url), content,
            new Dictionary<string, string> { ["x-cos-acl"] = everyone ? "public-read" : "private" });
    }

    public async Task<string> GetPublicUrlAsync(string path, TimeSpan? expires = null, CancellationToken ct = default) {
        var url = await GetUrlAsync(path, true, ct);

        using var request = new HttpRequestMessage { RequestUri = new Uri(url) };

        await _client.PrepareRequestAsync(request, false, expires: expires);

        return request.RequestUri.ToString();
    }

    public async Task<ObjectStorageFrontendUploadArguments> GetUploadArgumentsAsync(
        string            path,
        bool              everyone      = false,
        int?              contentLength = null,
        string?           contentType   = null,
        TimeSpan?         expires       = null,
        CancellationToken ct            = default) {
        var now        = DateTimeOffset.UtcNow;
        var expiration = now.Add(expires ?? TimeSpan.FromMinutes(30));
        var keyTime    = $"{now.ToUnixTimeSeconds()};{expiration.ToUnixTimeSeconds()}";

        var uri = await GetUrlAsync(string.Empty, true, ct);

        var arguments = new ObjectStorageFrontendUploadArguments {
            Action = uri,
            Data = {
                ["key"]              = path,
                ["acl"]              = everyone ? "public-read" : "private",
                ["q-sign-algorithm"] = "sha1",
                ["q-ak"]             = Options.SecretId,
                ["q-key-time"]       = keyTime,
            },
        };

        arguments.Data["policy"] = @$"{{
  ""expiration"": ""{
      expiration
      :yyyy-MM-ddTHH:mm:ss.fffZ}"",
  ""conditions"": [
    {{""acl"": ""{
        arguments.Data["acl"]
    }""}},
    {{""bucket"": ""{
        Options.BucketName
    }""}},";

        if (contentLength.HasValue) {
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
    {{""q-ak"": ""{
        arguments.Data["q-ak"]
    }""}},
    {{""q-sign-algorithm"": ""{
        arguments.Data["q-sign-algorithm"]
    }""}},
    {{""q-sign-time"": ""{
        arguments.Data["q-key-time"]
    }""}}
  ]
}}";

        arguments.Data["q-signature"] = _client.CalculateSignature(arguments.Data["policy"].Sha1().ToHex(), keyTime);
        arguments.Data["policy"]      = Convert.ToBase64String(Encoding.UTF8.GetBytes(arguments.Data["policy"]));

        return arguments;
    }

    public Task<string> GetUrlAsync(string path, bool cname = true, CancellationToken ct = default) {
        var protocol = Options.UseHttps ? "https" : "http";

        var prefix = !string.IsNullOrWhiteSpace(Options.CanonicalName) && cname
            ? $"{protocol}://{Options.CanonicalName}"
            : $"{protocol}://{Options.Endpoint}";

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
