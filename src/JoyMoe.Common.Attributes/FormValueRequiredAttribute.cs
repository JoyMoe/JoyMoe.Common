using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace JoyMoe.Common.Attributes;

/// <summary>
/// Represents an attribute that specifies which Form Data Field an action method will respond to.
/// </summary>
public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
{
    private readonly string _name;

    public FormValueRequiredAttribute(string name) {
        _name = name;
    }

    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action) {
        var ctx = routeContext.HttpContext;

        if (string.Equals(ctx.Request.Method, "GET", StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(ctx.Request.Method, "HEAD", StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(ctx.Request.Method, "DELETE", StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(ctx.Request.Method, "TRACE", StringComparison.InvariantCultureIgnoreCase)) {
            return false;
        }

        if (string.IsNullOrEmpty(ctx.Request.ContentType)) return false;

        if (!ctx.Request.ContentType.StartsWith("application/x-www-form-urlencoded",
                StringComparison.InvariantCultureIgnoreCase)) {
            return false;
        }

        return !string.IsNullOrEmpty(ctx.Request.Form[_name]);
    }
}
