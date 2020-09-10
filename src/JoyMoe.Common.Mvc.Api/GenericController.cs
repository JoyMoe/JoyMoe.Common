using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using JoyMoe.Common.EntityFrameworkCore.Models;
using JoyMoe.Common.EntityFrameworkCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    [ApiController]
    [GenericController]
    [Route("api/[controller]")]
    public class GenericController<TEntity, TRequest, TResponse> : ControllerBase
        where TEntity : class, IDataEntity
        where TRequest : class, IDataEntity
        where TResponse : class, IDataEntity
    {
        private readonly IRepository<TEntity> _repository;
        private readonly IGenericControllerInterceptor<TEntity> _interceptor;
        private readonly IMapper _mapper;

        public GenericController(IRepository<TEntity> repository, IGenericControllerInterceptor<TEntity> interceptor, IMapper mapper)
        {
            _repository = repository;
            _interceptor = interceptor;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] long? before = null, [FromQuery] int size = 10)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _interceptor.Query(HttpContext, User, predicate => _query(before, size, predicate)).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<IActionResult> _query(long? before, int size, Expression<Func<TEntity, bool>>? predicate)
        {
            var data = await _repository
                .PaginateAsync(before, size, predicate)
                .ConfigureAwait(false);

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Find(long id)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _interceptor.Find(HttpContext, User, () => _find(id)).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<IActionResult> _find(long id)
        {
            var entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TRequest? model)
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

            var entity = _mapRequest(model);

            var result = await _interceptor.Create(HttpContext, User, entity, _create).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<IActionResult> _create(TEntity entity)
        {
            await _repository.AddAsync(entity).ConfigureAwait(false);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return CreatedAtAction("Find", new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] TRequest? model)
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

            var entity = _mapRequest(model);

            var result = await _interceptor.Update(HttpContext, User, entity, _update).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<IActionResult> _update(TEntity entity)
        {
            _repository.Update(entity);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(long id)
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

            return await _interceptor.Remove(HttpContext, User, entity, _remove).ConfigureAwait(false);
        }

        private async Task<IActionResult> _remove(TEntity entity)
        {
            _repository.Remove(entity);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return NoContent();
        }

        private TEntity _mapRequest(TRequest model)
        {
            if (typeof(TRequest) != typeof(TEntity)) return _mapper.Map<TEntity>(model);

            if (model is TEntity entity)
            {
                return entity;
            }

            throw new NotSupportedException();
        }

        private IActionResult _mapResponse(IActionResult result)
        {
            if (!(result is ObjectResult or)) return result;

            or.Value = or.Value switch
            {
                TResponse => or.Value,
                TEntity entity => _mapper.Map<TResponse>(entity),
                IEnumerable<TEntity> entities => entities.Select(entity => _mapper.Map<TResponse>(entity)),
                _ => or.Value
            };

            return or;
        }
    }
}
