using System.Linq;
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

        public GenericController(IRepository<T> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] int size = 10, [FromQuery] long? before = null)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var data = await _repository.AsQueryable()
                .PaginateAsync(size, before)
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

            var record = await _repository.GetByIdAsync(id).ConfigureAwait(false);

            if (record == null)
            {
                return NotFound();
            }

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] T? record)
        {
            if (record == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            record.Id = default;

            await _repository.AddAsync(record).ConfigureAwait(false);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return CreatedAtAction("Find", new { id = record.Id }, record);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] T? record)
        {
            if (record == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            if (id != record.Id)
            {
                return BadRequest();
            }

            _repository.Update(record);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var record = await _repository.GetByIdAsync(id).ConfigureAwait(false);

            if (record == null)
            {
                return NotFound();
            }

            _repository.Remove(record);

            if (await _repository.CommitAsync().ConfigureAwait(false) == 0)
            {
                return Problem();
            }

            return NoContent();
        }
    }
}
