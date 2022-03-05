using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

#pragma warning disable CA1001 // IDisposable
namespace JoyMoe.Common.Storage.QCloud.Tests;

public class QCloudWebClientTests
{
    private const string Endpoint = "examplebucket-1250000000.cos.ap-beijing.myqcloud.com";

    private readonly QCloudWebClient _client = new(new QCloudStorageOptions
    {
        SecretId   = "AKIDQjz3ltompVjBni5LitkWHFlFpwkn9U5q",
        SecretKey  = "BQYIM75p8x0iWVFSIgqEKwFprpRSVHlz",
        Region     = "ap-beijing",
        BucketName = "examplebucket-1250000000"
    });

    [Fact]
    public void ShouldDeriveKeys() {
        var key = _client.DeriveKeys("1557989151;1557996351");
        Assert.Equal("eb2519b498b02ac213cb1f3d1a3d27a3b3c9bc5f", key);
    }

    [Fact]
    public async void ShouldGetObject() {
        var       mock   = CreateMockHandler();
        using var client = new HttpClient(mock.Object);
        _client.SetHttpClient(client);

        var time = new DateTimeOffset(2019, 05, 16, 06, 55, 53, TimeSpan.Zero);
        var uri = new Uri(
            $"http://{Endpoint}/exampleobject(%E8%85%BE%E8%AE%AF%E4%BA%91)?response-content-type=application%2Foctet-stream&response-cache-control=max-age%3D600");
        var result = await _client.GetAsync(uri,
                                            new Dictionary<string, string>
                                            {
                                                ["Date"] = "Thu, 16 May 2019 06:55:53 GMT"
                                            },
                                            time);
        Assert.Equal("Hello World!", result.Content.ReadAsStringAsync().GetAwaiter().GetResult());

        mock.Protected()
            .Verify("SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get &&
                                                         req.Headers.FindFirstValue("Authorization")!.Contains(
                                                             "q-header-list=date;host",
                                                             StringComparison.InvariantCultureIgnoreCase) &&
                                                         req.Headers.FindFirstValue("Authorization")!.Contains(
                                                             "q-url-param-list=response-cache-control;response-content-type",
                                                             StringComparison.InvariantCultureIgnoreCase) &&
                                                         req.Headers.FindFirstValue("Authorization")!.Contains(
                                                             "q-signature=01681b8c9d798a678e43b685a9f1bba0f6c0e012",
                                                             StringComparison.InvariantCultureIgnoreCase)),
                    ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async void ShouldPutObject() {
        var       mock   = CreateMockHandler();
        using var client = new HttpClient(mock.Object);
        _client.SetHttpClient(client);

        var time = new DateTimeOffset(2019, 05, 16, 06, 45, 51, TimeSpan.Zero);
        var uri  = new Uri($"http://{Endpoint}/exampleobject(%E8%85%BE%E8%AE%AF%E4%BA%91)");
        var data = new MemoryStream(Encoding.UTF8.GetBytes("ObjectContent"));

        using var content = new StreamContent(data);
        content.Headers.ContentLength = data.Length;
        content.Headers.ContentType   = new MediaTypeHeaderValue("text/plain");
        content.Headers.ContentMD5    = data.Md5();
        data.Seek(0, SeekOrigin.Begin);

        var result = await _client.PutAsync(uri,
                                            content,
                                            new Dictionary<string, string>
                                            {
                                                ["x-cos-acl"]        = "private",
                                                ["x-cos-grant-read"] = "uin=\"100000000011\"",
                                                ["Date"]             = "Thu, 16 May 2019 06:45:51 GMT"
                                            },
                                            time);
        Assert.Equal("Hello World!", result.Content.ReadAsStringAsync().GetAwaiter().GetResult());

        mock.Protected()
            .Verify("SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put &&
                                                         req.Headers.FindFirstValue("Authorization")!.Contains(
                                                             "q-header-list=content-length;content-md5;content-type;date;host;x-cos-acl;x-cos-grant-read",
                                                             StringComparison.InvariantCultureIgnoreCase) &&
                                                         req.Headers.FindFirstValue("Authorization")!.Contains(
                                                             "q-signature=3b8851a11a569213c17ba8fa7dcf2abec6935172",
                                                             StringComparison.InvariantCultureIgnoreCase)),
                    ItExpr.IsAny<CancellationToken>());
    }

    private static Mock<HttpMessageHandler> CreateMockHandler() {
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Default);

#pragma warning disable CA2000 // Dispose objects before losing scope
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK, Content = new StringContent("Hello World!")
        };
#pragma warning restore CA2000 // Dispose objects before losing scope

        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                                              ItExpr.IsAny<HttpRequestMessage>(),
                                              ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return mock;
    }
}
