using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    public interface IInterceptor<T> where T : class, IDataEntity
    {
        Task<ActionResult<IEnumerable<T>>> Query(GenericController<T> controller, Func<Expression<Func<T, bool>>?, Task<ActionResult<IEnumerable<T>>>> action);
        Task<ActionResult<T>> Find(GenericController<T> controller, Func<Task<ActionResult<T>>> action);
        Task<ActionResult<T>> Create(GenericController<T> controller, T entity, Func<T, Task<ActionResult<T>>> action);
        Task<ActionResult<T>> Update(GenericController<T> controller, T entity, Func<T, Task<ActionResult<T>>> action);
        Task<IActionResult> Delete(GenericController<T> controller, T entity, Func<T, Task<IActionResult>> action);
    }
}
