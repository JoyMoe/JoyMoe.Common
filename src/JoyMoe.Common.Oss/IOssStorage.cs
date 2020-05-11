using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JoyMoe.Common.Oss
{
    /// <summary>
    /// Object Storage Service client
    /// </summary>
    public interface IOssStorage
    {
        Task WriteStreamAsync(string path, Stream data, string mime, bool everyone = false, CancellationToken ct = default);

        Task<Dictionary<string, string>> GetUploadFormAsync(string path, bool everyone = false, CancellationToken ct = default);

        Task DeleteAsync(string path, CancellationToken ct = default);

        Task<string> GetUrlAsync(string path, CancellationToken ct = default);

        Task<string> GetPublicUrlAsync(string path, CancellationToken ct = default);
    }
}
