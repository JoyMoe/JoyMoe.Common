using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JoyMoe.Common.Attributes
{
    public class DomainPrefixFilter : Attribute, IResourceFilter
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
            if (!context.HttpContext.Request.Host.Host.StartsWith(_prefix))
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
