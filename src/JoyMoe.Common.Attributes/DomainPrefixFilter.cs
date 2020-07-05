using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JoyMoe.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class DomainPrefixFilter : Attribute, IResourceFilter
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private readonly string _prefix;

        public DomainPrefixFilter(string prefix)
        {
            _prefix = prefix;
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            //
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.HttpContext.Request.Host.Host.StartsWith(_prefix, StringComparison.InvariantCulture))
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
