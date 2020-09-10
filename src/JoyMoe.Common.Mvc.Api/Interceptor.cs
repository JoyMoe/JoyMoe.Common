using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    public class Interceptor<TEntity> : IInterceptor<TEntity>
        where TEntity : class, IDataEntity
    {
        public virtual Task<IActionResult> Query(HttpContext context, ClaimsPrincipal user,
            Func<Expression<Func<TEntity, bool>>?, Task<IActionResult>> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query(null);
        }

        public virtual Task<IActionResult> Find(HttpContext context, ClaimsPrincipal user,
            Func<Task<IActionResult>> find)
        {
            if (find == null)
            {
                throw new ArgumentNullException(nameof(find));
            }

            return find();
        }

        public virtual Task<IActionResult> Create(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<IActionResult>> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            return create(entity);
        }

        public virtual Task<IActionResult> Update(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<IActionResult>> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            return update(entity);
        }

        public virtual Task<IActionResult> Delete(HttpContext context, ClaimsPrincipal user, TEntity entity,
            Func<TEntity, Task<IActionResult>> delete)
        {
            if (delete == null)
            {
                throw new ArgumentNullException(nameof(delete));
            }

            return delete(entity);
        }
    }
}
