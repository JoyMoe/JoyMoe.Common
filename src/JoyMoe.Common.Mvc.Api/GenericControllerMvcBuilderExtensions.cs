using AutoMapper;
using JoyMoe.Common.Mvc.Api;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class GenericControllerMvcBuilderExtensions
{
    public static IMvcBuilder AddGenericControllers(this IMvcBuilder mvc, Action<GenericControllerBuilder> configure) {
        mvc.Services.TryAddScoped(typeof(IGenericControllerInterceptor<>), typeof(GenericControllerInterceptor<>));

        var builder = new GenericControllerBuilder(mvc);
        configure(builder);

        var config = new MapperConfiguration(builder.Mapper);
        var mapper = config.CreateMapper();
        mvc.Services.TryAddSingleton(mapper);

        return mvc;
    }
}
