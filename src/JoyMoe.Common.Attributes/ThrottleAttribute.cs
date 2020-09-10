using System;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace JoyMoe.Common.Attributes
{
    /// <summary>
    /// Add RateLimit to Actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrottleAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Throttle Pool
        /// </summary>
        public string Pool { get; set; } = null!;

        /// <summary>
        /// Throttle Times
        /// </summary>
        public int Times { get; set; }

        /// <summary>
        /// Throttle Seconds
        /// </summary>
        public int Seconds { get; set; }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cache = context.HttpContext.RequestServices.GetService<IDistributedCache>();

            var key = $"Throttle-{Pool}-{context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress}";

            var resetKey = $"{key}-reset";
            var timesKey = $"{key}-times";

            if (!int.TryParse(cache.GetString(timesKey), out var times))
            {
                times = 0;
            }

            times++;

            var reset = cache.GetString(resetKey);
            var resetAt = string.IsNullOrWhiteSpace(reset)
                ? DateTimeOffset.Now
                : DateTimeOffset.Parse(reset, CultureInfo.InvariantCulture);

            context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", Times.ToString(CultureInfo.InvariantCulture));
            context.HttpContext.Response.Headers.Add("X-RateLimit-Reset", resetAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
            context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", (Times - times).ToString(CultureInfo.InvariantCulture));

            if (times < Times)
            {
                if (times == 1)
                {
                    var expiration = DateTimeOffset.Now.AddSeconds(Seconds);

                    cache.SetString(timesKey, "1", new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = expiration
                    });

                    cache.SetString(resetKey, expiration.ToString(CultureInfo.InvariantCulture), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = expiration
                    });
                }
                else
                {
                    cache.SetString(timesKey, times.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.TooManyRequests);
            }

            base.OnActionExecuting(context);
        }
    }
}
