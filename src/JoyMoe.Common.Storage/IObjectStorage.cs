using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Storage;

/// <summary>
/// Object Storage Service client
/// </summary>
public interface IObjectStorage : IDisposable
{
    Task<string> DownloadAsync(string path, CancellationToken ct = default);

    Task DeleteAsync(string path, CancellationToken ct = default);

    Task UploadAsync(string path, Stream data, string mime, bool everyone = false, CancellationToken ct = default);

    Task<string> GetPublicUrlAsync(string path, CancellationToken ct = default);

    Task<ObjectStorageFrontendUploadArguments> GetUploadArgumentsAsync(
        string            path, bool everyone = false,
        int?              contentLength = null,
        string?           contentType   = null,
        CancellationToken ct            = default);

    Task<string> GetUrlAsync(string path, bool cname = true, CancellationToken ct = default);
}
