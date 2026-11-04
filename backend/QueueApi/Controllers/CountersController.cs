using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/counters")]
public class CountersController : ControllerBase
{
    private readonly ICounterRepository _counters;
    private readonly IQueueRepository _queues;

    public CountersController(ICounterRepository counters, IQueueRepository queues)
    {
        _counters = counters;
        _queues = queues;
    }

    // GET: api/queues/{queueId}/counters
    [HttpGet("/api/queues/{queueId:guid}/counters")]
    public async Task<IActionResult> GetByQueue(Guid queueId)
    {
        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        var counters = await _counters.GetByQueueIdAsync(queueId);

        return Ok(counters);
    }

    // GET: api/counters/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var counter = await _counters.GetByIdAsync(id);

        if (counter is null)
            return NotFound();

        return Ok(counter);
    }

    // POST: api/queues/{queueId}/counters
    [HttpPost("/api/queues/{queueId:guid}/counters")]
    public async Task<IActionResult> Create(Guid queueId, [FromBody] CreateCounterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Counter name is required.");

        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        var counter = new Counter
        {
            Id = Guid.NewGuid(),
            QueueId = queueId,
            Name = request.Name.Trim(),
            IsOpen = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _counters.CreateAsync(counter);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // PUT: api/counters/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCounterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Counter name is required.");

        var counter = new Counter
        {
            Id = id,
            Name = request.Name.Trim()
        };

        var updated = await _counters.UpdateAsync(counter);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // PATCH: api/counters/{id}/open
    [HttpPatch("{id:guid}/open")]
    public async Task<IActionResult> SetOpen(Guid id, [FromBody] SetOpenRequest request)
    {
        var updated = await _counters.SetOpenAsync(id, request.IsOpen);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/counters/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _counters.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
