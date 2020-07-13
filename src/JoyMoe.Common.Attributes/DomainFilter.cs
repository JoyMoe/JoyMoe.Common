using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JoyMoe.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class DomainFilter : Attribute, IResourceFilter
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private readonly Regex _regex;

        public DomainFilter(string regex)
        {
            _regex = new Regex(regex);
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

            if (!_regex.IsMatch(context.HttpContext.Request.Host.Host))
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
