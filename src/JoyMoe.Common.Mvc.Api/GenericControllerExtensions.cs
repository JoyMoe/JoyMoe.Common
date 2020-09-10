using System;
using AutoMapper;
using JoyMoe.Common.EntityFrameworkCore.Repositories;
using JoyMoe.Common.Mvc.Api;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class GenericControllerExtensions
    {
        public static IMvcBuilder AddGenericControllers(this IMvcBuilder builder, Action<GenericControllerOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new GenericControllerOptions();

            configure(options);

            foreach (var type in options.Types)
            {
                type.RequestType ??= type.EntityType;

                type.ResponseType ??= type.RequestType;
            }

            var mapperConfig = new MapperConfiguration(mc =>
            {
                if (options.Profiles.Count != 0)
                {
                    mc.AddProfiles(options.Profiles);

                    return;
                }

                foreach (var type in options.Types)
                {
                    if (type.RequestType != type.EntityType) mc.CreateMap(type.RequestType, type.EntityType);

                    if (type.ResponseType != type.EntityType)  mc.CreateMap(type.EntityType, type.ResponseType);
                }
            });

            var mapper = mapperConfig.CreateMapper();
            builder.Services.TryAddSingleton<IMapper>(mapper);
            builder.Services.TryAddScoped(typeof(IGenericControllerInterceptor<>), typeof(GenericControllerInterceptor<>));
            builder.Services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));

            builder.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider(options.Types));
            });

            return builder;
        }
    }
}
