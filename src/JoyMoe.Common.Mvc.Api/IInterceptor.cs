using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    public interface IInterceptor<TEntity>
        where TEntity : class, IDataEntity
    {
        Task<IActionResult> Query(HttpContext context, ClaimsPrincipal user, Func<Expression<Func<TEntity, bool>>?, Task<IActionResult>> query);
        Task<IActionResult> Find(HttpContext context, ClaimsPrincipal user, Func<Task<IActionResult>> find);
        Task<IActionResult> Create(HttpContext context, ClaimsPrincipal user, TEntity entity, Func<TEntity, Task<IActionResult>> create);
        Task<IActionResult> Update(HttpContext context, ClaimsPrincipal user, TEntity entity, Func<TEntity, Task<IActionResult>> update);
        Task<IActionResult> Remove(HttpContext context, ClaimsPrincipal user, TEntity entity, Func<TEntity, Task<IActionResult>> remove);
    }
}
