using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace JoyMoe.Common.Mvc.Api;

public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly Type _entityType;
    private readonly Type _requestType;
    private readonly Type _responseType;

    public GenericControllerFeatureProvider(Type entityType, Type requestType, Type responseType) {
        _entityType   = entityType;
        _requestType  = requestType;
        _responseType = responseType;
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature) {
        var typeName = _entityType.Name.Pluralize() + "Controller";

        // Check to see if there is a "real" controller for this class
        if (feature.Controllers.Any(t => t.Name == typeName)) return;

        // Create a generic controller for this type
        var controllerType = typeof(GenericController<,,>).MakeGenericType(_entityType, _requestType, _responseType)
                                                          .GetTypeInfo();

        feature.Controllers.Add(controllerType);
    }
}
