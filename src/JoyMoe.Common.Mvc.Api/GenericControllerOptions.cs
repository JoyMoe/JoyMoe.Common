using System;
using System.Collections.Generic;
using AutoMapper;
using JoyMoe.Common.EntityFrameworkCore.Models;

namespace JoyMoe.Common.Mvc.Api
{
    public class GenericControllerOptions
    {
        public GenericControllerTypes Types { get; } = new GenericControllerTypes();
        public List<Profile> Profiles { get; } = new List<Profile>();
    }

    public class GenericControllerTypes : List<GenericControllerType>
    {
        public void Add<TEntityType>()
        {
            Add(typeof(TEntityType));
        }

        public void Add<TEntityType, TRequest, TResponse>()
        {
            Add(typeof(TEntityType), typeof(TRequest), typeof(TResponse));
        }

        public void Add(Type entityType, Type? requestType = null, Type? responseType = null)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (!typeof(IDataEntity).IsAssignableFrom(entityType))
            {
                throw new NotSupportedException();
            }

            base.Add(new GenericControllerType(entityType, requestType, responseType));
        }
    }

    public class GenericControllerType
    {
        public Type EntityType { get; set; } = null!;
        public Type? RequestType { get; set; }
        public Type? ResponseType { get; set; }

        public GenericControllerType() {}

        public GenericControllerType(Type entityType, Type? requestType = null, Type? responseType = null)
        {
            EntityType = entityType;
            RequestType = requestType;
            ResponseType = responseType;
        }
    }
}
