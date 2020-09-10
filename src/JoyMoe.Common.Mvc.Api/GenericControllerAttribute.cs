using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JoyMoe.Common.Mvc.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GenericControllerAttribute : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (!controller.ControllerType.IsGenericType ||
                controller.ControllerType.GetGenericTypeDefinition() != typeof(GenericController<,,>))
            {
                return;
            }

            var entityType = controller.ControllerType.GetGenericArguments()[0];

            controller.ControllerName = entityType.Name.Pluralize();
        }
    }
}
