using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace JoyMoe.Common.Mvc.Api
{
    public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public List<GenericControllerType> Types { get; }

        public GenericControllerFeatureProvider(List<GenericControllerType> types)
        {
            Types = types;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            // Get the list of entities that we want to support for the generic controller
            foreach (var type in Types)
            {
                var typeName = type.EntityType.Name.Pluralize() + "Controller";

                // Check to see if there is a "real" controller for this class
                if (feature.Controllers.Any(t => t.Name == typeName)) continue;

                // Create a generic controller for this type
                var controllerType = typeof(GenericController<,,>).MakeGenericType(
                    type.EntityType,
                    type.RequestType!,
                    type.ResponseType!
                ).GetTypeInfo();
                feature.Controllers.Add(controllerType);
            }
        }
    }
}
