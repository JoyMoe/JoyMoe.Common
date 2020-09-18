using System;
using AutoMapper;
using AutoMapper.Configuration;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JoyMoe.Common.Mvc.Api
{
    public class GenericControllerBuilder
    {
        public GenericControllerBuilder(IMvcBuilder mvc)
        {
            Mvc = mvc;
        }

        public MapperConfigurationExpression Mapper { get; } = new MapperConfigurationExpression();

        public IMvcBuilder Mvc { get; }

        public GenericControllerBuilder Add(Profile profile)
        {
            Mapper.AddProfile(profile);

            return this;
        }

        public GenericControllerBuilder Add<TEntity>()
            where TEntity : class, IDataEntity
        {
            return Add(typeof(TEntity));
        }

        public GenericControllerBuilder Add<TEntity, TRequest, TResponse>()
            where TEntity : class, IDataEntity
            where TRequest : class, IIdentifier
            where TResponse : class, IIdentifier
        {
            return Add(typeof(TEntity), typeof(TRequest), typeof(TResponse));
        }

        public GenericControllerBuilder Add(Type entityType, Type? requestType = null, Type? responseType = null)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (!typeof(IDataEntity).IsAssignableFrom(entityType))
            {
                throw new NotSupportedException();
            }

            if (requestType != null && !typeof(IIdentifier).IsAssignableFrom(requestType))
            {
                throw new NotSupportedException();
            }

            if (responseType != null && !typeof(IIdentifier).IsAssignableFrom(responseType))
            {
                throw new NotSupportedException();
            }

            requestType ??= entityType;

            responseType ??= requestType;

            if (requestType != entityType) Mapper.CreateMap(requestType, entityType);

            if (responseType != entityType) Mapper.CreateMap(entityType, responseType);

            Mvc.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider(entityType, requestType, responseType));
            });

            return this;
        }

        public GenericControllerBuilder Add<TInterceptor, TEntity>()
            where TInterceptor : class, IGenericControllerInterceptor<TEntity>
            where TEntity : class, IDataEntity
        {
            Mvc.Services.TryAddScoped<IGenericControllerInterceptor<TEntity>, TInterceptor>();

            return this;
        }
    }
}
