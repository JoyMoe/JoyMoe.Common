using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JoyMoe.Common.Api.Mvc;

[AttributeUsage(AttributeTargets.Class)]
public class GenericControllerAttribute : Attribute, IControllerModelConvention
{
    public void Apply(ControllerModel controller) {
        if (!controller.ControllerType.IsGenericType ||
            controller.ControllerType.GetGenericTypeDefinition() != typeof(GenericController<,,>)) {
            return;
        }

        var entityType = controller.ControllerType.GetGenericArguments()[0];

        controller.ControllerName = entityType.Name.Pluralize();
    }
}
