using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

#pragma warning disable CA1001 // IDisposable
namespace JoyMoe.Common.Storage.S3.Tests
{
    public class S3WebClientTests
    {
        private const string Credential = "Credential=AKIAIOSFODNN7EXAMPLE/20130524/us-east-1/s3/aws4_request";
        private const string Endpoint = "examplebucket.s3.amazonaws.com";

        private readonly S3WebClient _client;
        private readonly DateTimeOffset _time = new DateTimeOffset(2013, 05, 24, 00, 00, 00, TimeSpan.Zero);

        public S3WebClientTests()
        {
            _client = new S3WebClient(new S3StorageOptions
            {
                AccessKey = "AKIAIOSFODNN7EXAMPLE",
                SecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
                Region = "us-east-1",
                BucketName = "examplebucket"
            });
        }

        [Fact]
        public void ShouldDeriveKeys()
        {
            var key = _client.DeriveKeys("20130524");
            Assert.Equal("dbb893acc010964918f1fd433add87c70e8b0db6be30c1fbeafefa5ec6ba8378", key.ToHex());
        }

        [Fact]
        public async void ShouldSignRequestInQuery()
        {
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://{Endpoint}/test.txt")
            };

            await _client.PrepareRequestAsync(request, false, _time).ConfigureAwait(false);

            Assert.Equal($"https://{Endpoint}/test.txt?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIOSFODNN7EXAMPLE%2F20130524%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20130524T000000Z&X-Amz-Expires=86400&X-Amz-SignedHeaders=host&X-Amz-Signature=aeeed9bbccd4d02ee5c0109b86d86835f995330da4c265957d157751f604d404", request.RequestUri.ToString());
        }

        [Fact]
        public async void ShouldGetObject()
        {
            var mock = CreateMockHandler();
            using var client = new HttpClient(mock.Object);
            _client.SetHttpClient(client);

            var uri = new Uri($"https://{Endpoint}/test.txt");
            var result = await _client.GetAsync(uri, new Dictionary<string, string>
            {
                ["Range"] = "bytes=0-9"
            }, _time).ConfigureAwait(false);
            Assert.Equal("Hello World!", result.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());

            mock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Headers.FindFirstValue("x-amz-date") == "20130524T000000Z" &&
                    req.Headers.Authorization!.Scheme == "AWS4-HMAC-SHA256" &&
                    req.Headers.Authorization!.Parameter == $"{Credential},SignedHeaders=host;range;x-amz-content-sha256;x-amz-date,Signature=f0e8bdb87c964420e857bd35b5d6ed310bd44f0170aba48dd91039c6036bdb41"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async void ShouldPutObject()
        {
            var mock = CreateMockHandler();
            using var client = new HttpClient(mock.Object);
            _client.SetHttpClient(client);

            var uri = new Uri($"https://{Endpoint}/test$file.text");

            using var content = new StringContent("Welcome to Amazon S3.");
            content.Headers.ContentLength = null;
            content.Headers.ContentType = null;

            var result = await _client.PutAsync(uri, content, new Dictionary<string, string>
            {
                ["x-amz-storage-class"] = "REDUCED_REDUNDANCY",
                ["Date"] = "Fri, 24 May 2013 00:00:00 GMT"
            }, _time).ConfigureAwait(false);
            Assert.Equal("Hello World!", result.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());

            mock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.Headers.FindFirstValue("x-amz-content-sha256") == "44ce7dd67c959e0d3524ffac1771dfbba87d2b6b4b4e99e42034a8b803f8b072" &&
                    req.Headers.FindFirstValue("x-amz-date") == "20130524T000000Z" &&
                    req.Headers.Authorization!.Scheme == "AWS4-HMAC-SHA256" &&
                    req.Headers.Authorization!.Parameter == $"{Credential},SignedHeaders=date;host;x-amz-content-sha256;x-amz-date;x-amz-storage-class,Signature=98ad721746da40c64f1a55b78f14c238d841ea1380cd77a1b5971af0ece108bd"),
                ItExpr.IsAny<CancellationToken>());
        }

        private static Mock<HttpMessageHandler> CreateMockHandler()
        {
            var mock = new Mock<HttpMessageHandler>(MockBehavior.Default);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Hello World!")
            };
#pragma warning restore CA2000 // Dispose objects before losing scope

            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            return mock;
        }
    }
}
