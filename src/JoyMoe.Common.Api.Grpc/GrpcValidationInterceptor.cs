using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace JoyMoe.Common.Api.Grpc;

public class GrpcValidationInterceptor : Interceptor
{
    private readonly IServiceProvider _provider;

    public GrpcValidationInterceptor(IServiceProvider provider) {
        _provider = provider;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest                               request,
        ServerCallContext                      context,
        UnaryServerMethod<TRequest, TResponse> continuation) {
        await ValidateRequestAsync(request);
        return await continuation(request, context);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest                                         request,
        IServerStreamWriter<TResponse>                   responseStream,
        ServerCallContext                                context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation) {
        await ValidateRequestAsync(request);
        await continuation(request, responseStream, context);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest>                     requestStream,
        ServerCallContext                                context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation) {
        var reader = new GrpcValidationAsyncStreamReader<TRequest>(requestStream, ValidateRequestAsync);
        return await continuation(reader, context);
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest>                     requestStream,
        IServerStreamWriter<TResponse>                   responseStream,
        ServerCallContext                                context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation) {
        var reader = new GrpcValidationAsyncStreamReader<TRequest>(requestStream, ValidateRequestAsync);
        await continuation(reader, responseStream, context);
    }

    private async Task ValidateRequestAsync<TRequest>(TRequest request)
        where TRequest : class {
        var validator = _provider.GetService<IValidator<TRequest>>();
        if (validator == null) return;

        var metadata = new Metadata();

        await foreach (var (k, v) in Validation.ValidateAsync(validator, request)) {
            metadata.Add(k, v);
        }

        if (metadata.Count == 0) return;

        throw new StatusException(StatusCode.InvalidArgument, "invalid request", metadata);
    }
}
