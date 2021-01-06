using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JoyMoe.Common.Data;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Mvc.Api
{
    [ApiController]
    [GenericController]
    [Route("api/[controller]")]
    public class GenericController<TEntity, TRequest, TResponse> : ControllerBase
        where TEntity : class, IDataEntity
        where TRequest : class, IIdentifier
        where TResponse : class, IIdentifier
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
        public async Task<ActionResult<IEnumerable<TResponse>>> Query([FromQuery] long? before = null, [FromQuery] int size = 10)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _interceptor.Query(HttpContext, User, (predicate, values) => _query(before, size, predicate, values ?? Array.Empty<object>())).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<ActionResult<IEnumerable<TEntity>>> _query(long? before, int size, string? predicate, params object[] values)
        {
            var entities = await _repository
                .PaginateAsync(e => e.Id, before, size, predicate: predicate, values: values)
                .ConfigureAwait(false);

            return entities.ToArray();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TResponse>> Find(long id)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _interceptor.Find(HttpContext, User, () => _find(id)).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<ActionResult<TEntity>> _find(long id)
        {
            var entity = await _repository.FindAsync(e => e.Id, id).ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound();
            }

            return entity;
        }

        [HttpPost]
        public async Task<ActionResult<TResponse>> Create([FromBody] TRequest? request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            request.Id = default;

            var entity = _mapRequest(request);

            var result = await _interceptor.Create(HttpContext, User, entity, _create).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<ActionResult<TEntity>> _create(TEntity entity)
        {
            await _repository.AddAsync(entity).ConfigureAwait(false);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return CreatedAtAction("Find", new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TResponse>> Update(long id, [FromBody] TRequest? request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest();
            }

            var entity = _mapRequest(request);

            var result = await _interceptor.Update(HttpContext, User, entity, _update).ConfigureAwait(false);

            return _mapResponse(result);
        }

        private async Task<ActionResult<TEntity>> _update(TEntity entity)
        {
            await _repository.UpdateAsync(entity).ConfigureAwait(false);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return entity;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Remove(long id)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var entity = await _repository.FindAsync(e => e.Id, id).ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound();
            }

            return await _interceptor.Remove(HttpContext, User, entity, _remove).ConfigureAwait(false);
        }

        private async Task<ActionResult> _remove(TEntity entity)
        {
            await _repository.RemoveAsync(entity).ConfigureAwait(false);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return NoContent();
        }

        private TEntity _mapRequest(TRequest request)
        {
            if (typeof(TRequest) != typeof(TEntity)) return _mapper.Map<TEntity>(request);

            if (request is TEntity entity)
            {
                return entity;
            }

            throw new NotSupportedException();
        }

        private TResponse _mapResponse(TEntity entity)
        {
            if (typeof(TEntity) != typeof(TResponse)) return _mapper.Map<TResponse>(entity);

            if (entity is TResponse response)
            {
                return response;
            }

            throw new NotSupportedException();
        }

        private IEnumerable<TResponse> _mapResponse(IEnumerable<TEntity> entities)
        {
            if (typeof(TEntity) != typeof(TResponse)) return entities.Select(e => _mapper.Map<TResponse>(e));

            if (entities is IEnumerable<TResponse> responses)
            {
                return responses;
            }

            throw new NotSupportedException();
        }

        private ActionResult<TResponse> _mapResponse(ActionResult<TEntity> result)
        {
            if (result.Result == null) return new ActionResult<TResponse>(_mapResponse(result.Value));

            if (!(result.Result is ObjectResult or) || !(or.Value is TEntity entity)) return result.Result;

            or.Value = _mapResponse(entity);

            return new ActionResult<TResponse>(or);

        }

        private ActionResult<IEnumerable<TResponse>> _mapResponse(ActionResult<IEnumerable<TEntity>> result)
        {
            if (result.Result == null) return new ActionResult<IEnumerable<TResponse>>(_mapResponse(result.Value));

            if (!(result.Result is ObjectResult or) || !(or.Value is IEnumerable<TEntity> entities)) return result.Result;

            or.Value = _mapResponse(entities);

            return new ActionResult<IEnumerable<TResponse>>(or);
        }
    }
}
