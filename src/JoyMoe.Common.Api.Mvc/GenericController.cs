using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using FluentValidation;
using JoyMoe.Common.Abstractions;
using JoyMoe.Common.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace JoyMoe.Common.Api.Mvc;

[ApiController]
[GenericController]
[Route("api/[controller]")]
public class GenericController<TEntity, TRequest, TResponse> : ControllerBase
    where TEntity : class, IDataEntity
    where TRequest : class, IIdentifier
    where TResponse : class, IIdentifier
{
    private readonly IServiceProvider _provider;

    private readonly IRepository<TEntity>                   _repository;
    private readonly IGenericControllerInterceptor<TEntity> _interceptor;
    private readonly IMapper                                _mapper;

    public GenericController(
        IServiceProvider                       provider,
        IRepository<TEntity>                   repository,
        IGenericControllerInterceptor<TEntity> interceptor,
        IMapper                                mapper) {
        _provider    = provider;
        _repository  = repository;
        _interceptor = interceptor;
        _mapper      = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<CursorPaginationResponse<long, TResponse>>> Query(
        [FromQuery] long? cursor,
        [FromQuery] int   size = 10) {
        var result = await _interceptor.Query(HttpContext, User, predicate => _query(cursor, size, predicate));

        return _mapResponse(result);
    }

    private async Task<ActionResult<CursorPaginationResponse<long, TEntity>>> _query(
        long?                            cursor,
        int                              size,
        Expression<Func<TEntity, bool>>? predicate) {
        return await _repository.PaginateAsync(e => e.Id, predicate, cursor, size);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TResponse>> Find(long id) {
        var result = await _interceptor.Find(HttpContext, User, () => _find(id));

        return _mapResponse(result);
    }

    private async Task<ActionResult<TEntity>> _find(long id) {
        var entity = await _repository.FindAsync(e => e.Id, id);

        if (entity == null) return NotFound();

        return entity;
    }

    [HttpPost]
    public async Task<ActionResult<TResponse>> Create([FromBody] TRequest request) {
        var validation = await ValidateAsync(request);
        if (validation != null) return validation;

        request.Id = default;

        var entity = _mapRequest(request);

        var result = await _interceptor.Create(HttpContext, User, entity, _create);

        return _mapResponse(result);
    }

    private async Task<ActionResult<TEntity>> _create(TEntity entity) {
        await _repository.AddAsync(entity);

        if (await _repository.CommitAsync() == 0) return Problem();

        return CreatedAtAction("Find", new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TResponse>> Update(long id, [FromBody] TRequest request) {
        var validation = await ValidateAsync(request);
        if (validation != null) return validation;

        if (id != request.Id) return BadRequest();

        var entity = _mapRequest(request);

        var result = await _interceptor.Update(HttpContext, User, entity, _update);

        return _mapResponse(result);
    }

    private async Task<ActionResult<TEntity>> _update(TEntity entity) {
        await _repository.UpdateAsync(entity);

        if (await _repository.CommitAsync() == 0) return Problem();

        return entity;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Remove(long id) {
        var entity = await _repository.FindAsync(e => e.Id, id);

        if (entity == null) return NotFound();

        return await _interceptor.Remove(HttpContext, User, entity, _remove);
    }

    private async Task<ActionResult> _remove(TEntity entity) {
        await _repository.RemoveAsync(entity);

        if (await _repository.CommitAsync() == 0) return Problem();

        return NoContent();
    }

    private async Task<ActionResult<TResponse>?> ValidateAsync(TRequest request) {
        var validator = _provider.GetService<IValidator<TRequest>>();
        if (validator == null) return null;

        var results = await validator.ValidateAsync(request);
        if (results.IsValid || !results.Errors.Any()) return null;

        await foreach (var (k, v) in Validation.ValidateAsync(validator, request)) {
            Response.Headers.Add(k, v);
        }

        if (!Response.Headers.ContainsKey("errors")) return null;

        return StatusCode((int)HttpStatusCode.UnprocessableEntity, new {
            error             = "invalid_request", //
            error_description = "The request is invalid.",
        });
    }

    private TEntity _mapRequest(TRequest request) {
        if (typeof(TRequest) != typeof(TEntity)) return _mapper.Map<TEntity>(request);

        if (request is TEntity entity) return entity;

        throw new NotSupportedException();
    }

    private TResponse _mapResponse(TEntity entity) {
        if (typeof(TEntity) != typeof(TResponse)) return _mapper.Map<TResponse>(entity);

        if (entity is TResponse response) return response;

        throw new NotSupportedException();
    }

    private ICollection<TResponse> _mapResponse(ICollection<TEntity> entities) {
        if (typeof(TEntity) != typeof(TResponse)) return _mapper.Map<ICollection<TResponse>>(entities);

        if (entities is ICollection<TResponse> responses) return responses;

        throw new NotSupportedException();
    }

    private CursorPaginationResponse<long, TResponse> _mapResponse(CursorPaginationResponse<long, TEntity> result) {
        if (typeof(TEntity) != typeof(TResponse)) {
            ICollection<TResponse>? data = null;

            if (result.Data != null) data = _mapResponse(result.Data);

            return new CursorPaginationResponse<long, TResponse> { Next = result.Next, Data = data };
        }

        if (result is CursorPaginationResponse<long, TResponse> response) return response;

        throw new NotSupportedException();
    }

    private ActionResult<TResponse> _mapResponse(ActionResult<TEntity> result) {
        if (result.Result == null) return new ActionResult<TResponse>(_mapResponse(result.Value!));

        if (result.Result is not ObjectResult { Value: TEntity entity } or) return result.Result;

        or.Value = _mapResponse(entity);

        return new ActionResult<TResponse>(or);
    }

    private ActionResult<CursorPaginationResponse<long, TResponse>> _mapResponse(
        ActionResult<CursorPaginationResponse<long, TEntity>> result) {
        if (result.Result == null) {
            return new ActionResult<CursorPaginationResponse<long, TResponse>>(_mapResponse(result.Value!));
        }

        if (result.Result is not ObjectResult { Value: CursorPaginationResponse<long, TEntity> entities } or) {
            return result.Result;
        }

        or.Value = _mapResponse(entities);

        return new ActionResult<CursorPaginationResponse<long, TResponse>>(or);
    }
}
