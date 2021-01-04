using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using JoyMoe.Common.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    public class GenericControllerInterceptor<TEntity> : IGenericControllerInterceptor<TEntity>
        where TEntity : class, IDataEntity
    {
        public virtual Task<ActionResult<IEnumerable<TEntity>>> Query(HttpContext context, ClaimsPrincipal user,
            Func<string?, List<object>?, Task<ActionResult<IEnumerable<TEntity>>>> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query(null, null);
        }

        public virtual Task<ActionResult<TEntity>> Find(HttpContext context, ClaimsPrincipal user,
            Func<Task<ActionResult<TEntity>>> find)
        {
            if (find == null)
            {
                throw new ArgumentNullException(nameof(find));
            }

            return find();
        }

        public virtual Task<ActionResult<TEntity>> Create(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<ActionResult<TEntity>>> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            return create(entity);
        }

        public virtual Task<ActionResult<TEntity>> Update(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<ActionResult<TEntity>>> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            return update(entity);
        }

        public virtual Task<ActionResult> Remove(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<ActionResult>> remove)
        {
            if (remove == null)
            {
                throw new ArgumentNullException(nameof(remove));
            }

            return remove(entity);
        }
    }
}
