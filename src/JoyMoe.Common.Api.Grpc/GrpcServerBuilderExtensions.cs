using Grpc.AspNetCore.Server;
using JoyMoe.Common.Api.Grpc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class GrpcServerBuilderExtensions
{
    public static IGrpcServerBuilder EnableValidation(
        this IGrpcServerBuilder    builder,
        Action<IServiceCollection> configure) {
        builder.Services.Configure<GrpcServiceOptions>(options => {
            options.Interceptors.Add<GrpcValidationInterceptor>();
        });

        configure(builder.Services);

        return builder;
    }

    public static IGrpcServerBuilder EnableDeprecation(this IGrpcServerBuilder builder) {
        builder.Services.Configure<GrpcServiceOptions>(options => {
            options.Interceptors.Add<GrpcDeprecationInterceptor>();
        });

        return builder;
    }
}
