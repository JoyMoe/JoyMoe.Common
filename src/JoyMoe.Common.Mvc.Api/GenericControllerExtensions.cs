using System;
using System.Collections.Generic;
using JoyMoe.Common.Mvc.Api;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class GenericControllerExtensions
    {
        public static IMvcBuilder AddGenericControllers(this IMvcBuilder mvcBuilder, Action<List<Type>> entities)
        {
            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var types = new List<Type>();

            entities(types);

            mvcBuilder.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider(types));
            });

            return mvcBuilder;
        }
    }
}
