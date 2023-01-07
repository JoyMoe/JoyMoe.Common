using System.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace JoyMoe.Common.Api.Grpc;

public class GrpcDeprecationInterceptor : Interceptor
{
    private readonly IServiceProvider _provider;

    public GrpcDeprecationInterceptor(IServiceProvider provider) {
        _provider = provider;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest                               request,
        ServerCallContext                      context,
        UnaryServerMethod<TRequest, TResponse> continuation) {
        await CheckDeprecationAsync(context, continuation.Method);
        return await continuation(request, context);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest                                         request,
        IServerStreamWriter<TResponse>                   responseStream,
        ServerCallContext                                context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation) {
        await CheckDeprecationAsync(context, continuation.Method);
        await continuation(request, responseStream, context);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest>                     requestStream,
        ServerCallContext                                context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation) {
        await CheckDeprecationAsync(context, continuation.Method);
        return await continuation(requestStream, context);
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest>                     requestStream,
        IServerStreamWriter<TResponse>                   responseStream,
        ServerCallContext                                context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation) {
        await CheckDeprecationAsync(context, continuation.Method);
        await continuation(requestStream, responseStream, context);
    }

    private static Task CheckDeprecationAsync(ServerCallContext context, MethodInfo method) {
        var deprecation = method.GetCustomAttribute<DeprecationAttribute>();
        if (deprecation == null) return Task.CompletedTask;

        var metadata = new Metadata { { "deprecation", deprecation.DeprecationDate?.ToString("R") ?? "true" } };

        if (!string.IsNullOrWhiteSpace(deprecation.Documentation)) {
            metadata.Add("link", $"<{deprecation.Documentation}>; rel=\"deprecation\"; type=\"text/html\"");
        }

        if (deprecation.SuccessorVersions?.Length > 0) {
            foreach (var version in deprecation.SuccessorVersions) {
                metadata.Add("link", $"<{version}>; rel=\"successor-version\"");
            }
        }

        if (!string.IsNullOrWhiteSpace(deprecation.LatestVersion)) {
            metadata.Add("link", $"<{deprecation.LatestVersion}>; rel=\"latest-version\"");
        }

        if (deprecation.Alternates?.Length > 0) {
            foreach (var version in deprecation.Alternates) {
                metadata.Add("link", $"<{version}>; rel=\"alternate\"");
            }
        }

        if (deprecation.IsDeprecated) {
            throw new StatusException(StatusCode.Unavailable, "resource deprecated", metadata);
        }

        foreach (var entry in metadata) {
            context.ResponseTrailers.Add(entry);
        }

        return Task.CompletedTask;
    }
}
