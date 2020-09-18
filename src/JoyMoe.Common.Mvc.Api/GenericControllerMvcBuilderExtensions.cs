using System;
using AutoMapper;
using JoyMoe.Common.EntityFrameworkCore.Repositories;
using JoyMoe.Common.Mvc.Api;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class GenericControllerMvcBuilderExtensions
    {
        public static IMvcBuilder AddGenericControllers(this IMvcBuilder mvc, Action<GenericControllerBuilder> configure)
        {
            if (mvc == null)
            {
                throw new ArgumentNullException(nameof(mvc));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            mvc.Services.TryAddScoped(typeof(IGenericControllerInterceptor<>), typeof(GenericControllerInterceptor<>));
            mvc.Services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));

            var builder = new GenericControllerBuilder(mvc);
            configure(builder);

            var config = new MapperConfiguration(builder.Mapper);
            var mapper = config.CreateMapper();
            mvc.Services.TryAddSingleton<IMapper>(mapper);

            return mvc;
        }
    }
}
