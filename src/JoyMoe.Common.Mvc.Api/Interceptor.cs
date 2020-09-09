using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    public class Interceptor<T> : IInterceptor<T> where T : class, IDataEntity
    {
        public virtual Task<ActionResult<IEnumerable<T>>> Query(GenericController<T> controller,
            Func<Expression<Func<T, bool>>?, Task<ActionResult<IEnumerable<T>>>> action) => action(null);

        public virtual Task<ActionResult<T>> Find(GenericController<T> controller,
            Func<Task<ActionResult<T>>> action) => action();

        public virtual Task<ActionResult<T>> Create(GenericController<T> controller, T entity,
            Func<T, Task<ActionResult<T>>> action) => action(entity);

        public virtual Task<ActionResult<T>> Update(GenericController<T> controller, T entity,
            Func<T, Task<ActionResult<T>>> action) => action(entity);

        public virtual Task<IActionResult> Delete(GenericController<T> controller, T entity,
            Func<T, Task<IActionResult>> action) => action(entity);
    }
}
