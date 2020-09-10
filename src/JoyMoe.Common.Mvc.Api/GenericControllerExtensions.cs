using System;
using System.Collections.Generic;
using AutoMapper;
using JoyMoe.Common.EntityFrameworkCore.Repositories;
using JoyMoe.Common.Mvc.Api;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class GenericControllerExtensions
    {
        public static IMvcBuilder AddGenericControllers(this IMvcBuilder mvcBuilder, Type mapper, Action<Dictionary<Type, (Type, Type)>> entities)
        {
            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var types = new Dictionary<Type, (Type, Type)>();

            entities(types);

            mvcBuilder.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider(types));
            });

            mvcBuilder.Services.AddAutoMapper(mapper);
            mvcBuilder.Services.AddScoped(typeof(IInterceptor<>), typeof(Interceptor<>));
            mvcBuilder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return mvcBuilder;
        }
    }
}
