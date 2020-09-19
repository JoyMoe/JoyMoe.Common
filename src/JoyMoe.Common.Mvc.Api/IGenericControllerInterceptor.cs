using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    public interface IGenericControllerInterceptor<TEntity>
        where TEntity : class, IDataEntity
    {
        Task<ActionResult<IEnumerable<TEntity>>> Query(HttpContext context, ClaimsPrincipal user,
            Func<Expression<Func<TEntity, bool>>?, Task<ActionResult<IEnumerable<TEntity>>>> query);

        Task<ActionResult<TEntity>> Find(HttpContext context, ClaimsPrincipal user,
            Func<Task<ActionResult<TEntity>>> find);

        Task<ActionResult<TEntity>> Create(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<ActionResult<TEntity>>> create);

        Task<ActionResult<TEntity>> Update(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<ActionResult<TEntity>>> update);

        Task<ActionResult> Remove(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<ActionResult>> remove);
    }
}
