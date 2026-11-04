using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/queues")]
public class QueuesController : ControllerBase
{
    private static readonly Regex TicketPrefixPattern = new("^[A-Z]{1,10}$", RegexOptions.Compiled);

    private readonly IQueueRepository _repository;

    public QueuesController(IQueueRepository repository)
    {
        _repository = repository;
    }

    // GET: api/queues?organizationId=
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? organizationId)
    {
        var queues = await _repository.GetAllAsync(organizationId);

        return Ok(queues);
    }

    // GET: api/queues/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var queue = await _repository.GetByIdAsync(id);

        if (queue is null)
            return NotFound();

        return Ok(queue);
    }

    // POST: api/queues
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQueueRequest request)
    {
        if (request.OrganizationId == Guid.Empty)
            return BadRequest("A valid organization ID is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Queue name is required.");

        var prefix = request.TicketPrefix?.Trim() ?? string.Empty;

        if (!TicketPrefixPattern.IsMatch(prefix))
            return BadRequest("Ticket prefix must be 1 to 10 uppercase letters.");

        var queue = new Queue
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            TicketPrefix = prefix,
            IsOpen = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(queue);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // PUT: api/queues/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQueueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Queue name is required.");

        var queue = new Queue
        {
            Id = id,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty
        };

        var updated = await _repository.UpdateAsync(queue);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // PATCH: api/queues/{id}/open
    [HttpPatch("{id:guid}/open")]
    public async Task<IActionResult> SetOpen(Guid id, [FromBody] SetOpenRequest request)
    {
        var updated = await _repository.SetOpenAsync(id, request.IsOpen);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/queues/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
