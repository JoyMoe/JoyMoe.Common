using System;
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
        public string Pool { get; set; }

        /// <summary>
        /// Throttle Times
        /// </summary>
        public int Times { get; set; }

        /// <summary>
        /// Throttle Seconds
        /// </summary>
        public int Seconds { get; set; }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext c)
        {
            var cache = c.HttpContext.RequestServices.GetService<IDistributedCache>();

            var key = $"Throttle-{Pool}-{c.HttpContext.Request.HttpContext.Connection.RemoteIpAddress}";

            var resetKey = $"{key}-reset";
            var timesKey = $"{key}-times";

            int.TryParse(cache.GetString(timesKey), out var times);

            times++;

            var reset = cache.GetString(resetKey);
            var resetAt = string.IsNullOrWhiteSpace(reset)
                ? DateTimeOffset.Now
                : DateTimeOffset.Parse(reset);

            c.HttpContext.Response.Headers.Add("X-RateLimit-Limit", Times.ToString());
            c.HttpContext.Response.Headers.Add("X-RateLimit-Reset", resetAt.ToUnixTimeSeconds().ToString());
            c.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", (Times - times).ToString());

            if (times < Times)
            {
                if (times == 1)
                {
                    var expiration = DateTimeOffset.Now.AddSeconds(Seconds);

                    cache.SetString(timesKey, "1", new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = expiration
                    });

                    cache.SetString(resetKey, expiration.ToString(), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = expiration
                    });
                }
                else
                {
                    cache.SetString(timesKey, times.ToString());
                }
            }
            else
            {
                c.Result = new StatusCodeResult((int)HttpStatusCode.TooManyRequests);
            }

            base.OnActionExecuting(c);
        }
    }
}
