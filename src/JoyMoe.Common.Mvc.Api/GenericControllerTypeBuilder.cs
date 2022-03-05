using AutoMapper;
using JoyMoe.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JoyMoe.Common.Mvc.Api;

public class GenericControllerTypeBuilder<TEntity, TRequest, TResponse> : GenericControllerBuilder
    where TEntity : class, IDataEntity
    where TRequest : class, IIdentifier
    where TResponse : class, IIdentifier
{
    public GenericControllerTypeBuilder(IMvcBuilder mvc, MapperConfigurationExpression mapper) : base(mvc, mapper) { }

    public GenericControllerBuilder With<TInterceptor>()
        where TInterceptor : class, IGenericControllerInterceptor<TEntity> {
        Mvc.Services.TryAddScoped<IGenericControllerInterceptor<TEntity>, TInterceptor>();

        return this;
    }
}
