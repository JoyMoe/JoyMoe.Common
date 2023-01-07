using AutoMapper;
using JoyMoe.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JoyMoe.Common.Api.Mvc;

public class GenericControllerBuilder
{
    public GenericControllerBuilder(IMvcBuilder mvc, MapperConfigurationExpression mapper) {
        Mvc    = mvc;
        Mapper = mapper;
    }

    public GenericControllerBuilder(IMvcBuilder mvc) {
        Mvc    = mvc;
        Mapper = new MapperConfigurationExpression();
    }

    public MapperConfigurationExpression Mapper { get; }

    public IMvcBuilder Mvc { get; }

    public GenericControllerBuilder Add(Profile profile) {
        Mapper.AddProfile(profile);

        return this;
    }

    public GenericControllerTypeBuilder<TEntity, TEntity, TEntity> Add<TEntity>() where TEntity : class, IDataEntity {
        return Add<TEntity, TEntity, TEntity>();
    }

    public GenericControllerTypeBuilder<TEntity, TRequest, TResponse> Add<TEntity, TRequest, TResponse>()
        where TEntity : class, IDataEntity
        where TRequest : class, IIdentifier
        where TResponse : class, IIdentifier {
        if (typeof(TRequest) != typeof(TEntity)) Mapper.CreateMap<TRequest, TEntity>();

        if (typeof(TEntity) != typeof(TResponse)) Mapper.CreateMap<TEntity, TResponse>();

        Mvc.ConfigureApplicationPartManager(manager => {
            manager.FeatureProviders.Add(
                new GenericControllerFeatureProvider(typeof(TEntity), typeof(TRequest), typeof(TResponse)));
        });

        return new GenericControllerTypeBuilder<TEntity, TRequest, TResponse>(Mvc, Mapper);
    }
}
