using System.Linq.Expressions;
using System.Security.Claims;
using JoyMoe.Common.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Api.Mvc;

public class GenericControllerInterceptor<TEntity> : IGenericControllerInterceptor<TEntity>
    where TEntity : class, IDataEntity
{
    public virtual Task<ActionResult<CursorPaginationResponse<long, TEntity>>> Query(
        HttpContext                                                                                         context,
        ClaimsPrincipal                                                                                     user,
        Func<Expression<Func<TEntity, bool>>?, Task<ActionResult<CursorPaginationResponse<long, TEntity>>>> query) {
        return query(null);
    }

    public virtual Task<ActionResult<TEntity>> Find(
        HttpContext                       context,
        ClaimsPrincipal                   user,
        Func<Task<ActionResult<TEntity>>> find) {
        return find();
    }

    public virtual Task<ActionResult<TEntity>> Create(
        HttpContext                                context,
        ClaimsPrincipal                            user,
        TEntity                                    entity,
        Func<TEntity, Task<ActionResult<TEntity>>> create) {
        return create(entity);
    }

    public virtual Task<ActionResult<TEntity>> Update(
        HttpContext                                context,
        ClaimsPrincipal                            user,
        TEntity                                    entity,
        Func<TEntity, Task<ActionResult<TEntity>>> update) {
        return update(entity);
    }

    public virtual Task<ActionResult> Remove(
        HttpContext                       context,
        ClaimsPrincipal                   user,
        TEntity                           entity,
        Func<TEntity, Task<ActionResult>> remove) {
        return remove(entity);
    }
}
