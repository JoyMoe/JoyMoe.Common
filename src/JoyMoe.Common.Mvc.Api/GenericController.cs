using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyMoe.Common.EntityFrameworkCore.Models;
using JoyMoe.Common.EntityFrameworkCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    [ApiController]
    [GenericController]
    [Route("api/[controller]")]
    public class GenericController<T> : ControllerBase where T : class, IDataEntity
    {
        private readonly IRepository<T> _repository;
        private readonly IInterceptor<T> _interceptor;

        public GenericController(IRepository<T> repository, IInterceptor<T> interceptor)
        {
            _repository = repository;
            _interceptor = interceptor;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<T>>> Query([FromQuery] long? before = null, [FromQuery] int size = 10)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            return await _interceptor.Query(this, predicate => _query(before, size, predicate)).ConfigureAwait(false);
        }

        private async Task<ActionResult<IEnumerable<T>>> _query(long? before, int size, Expression<Func<T, bool>>? predicate)
        {
            var data = await _repository
                .PaginateAsync(before, size, predicate)
                .ConfigureAwait(false);

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<T>> Find(long id)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            return await _interceptor.Find(this, () => _find(id)).ConfigureAwait(false);
        }

        private async Task<ActionResult<T>> _find(long id)
        {
            var entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<T>> Create([FromBody] T? model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            model.Id = default;

            return await _interceptor.Create(this, model, _create).ConfigureAwait(false);
        }

        private async Task<ActionResult<T>> _create(T entity)
        {
            await _repository.AddAsync(entity).ConfigureAwait(false);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return CreatedAtAction("Find", new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<T>> Update(long id, [FromBody] T? model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            return await _interceptor.Update(this, model, _update).ConfigureAwait(false);
        }

        private async Task<ActionResult<T>> _update(T entity)
        {
            _repository.Update(entity);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound();
            }

            return await _interceptor.Delete(this, entity, _delete).ConfigureAwait(false);
        }

        private async Task<IActionResult> _delete(T entity)
        {
            _repository.Remove(entity);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return NoContent();
        }
    }
}
