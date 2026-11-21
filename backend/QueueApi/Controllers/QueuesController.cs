using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Infrastructure;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;
using QueueApi.Services;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/queues")]
public class QueuesController : ControllerBase
{
    private static readonly Regex TicketPrefixPattern = new("^[A-Z]{1,10}$", RegexOptions.Compiled);

    private readonly IQueueRepository _repository;
    private readonly IProfileLookup _profiles;

    public QueuesController(IQueueRepository repository, IProfileLookup profiles)
    {
        _repository = repository;
        _profiles = profiles;
    }

    // GET: api/queues?organizationId=
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? organizationId)
    {
        var queues = await _repository.GetAllAsync(organizationId);
        var names = await _profiles.GetOrganizationNamesAsync(queues.Select(queue => queue.OrganizationId));

        return Ok(queues
            .Select(queue => QueueResponse.From(queue, NameOrNull(names, queue.OrganizationId)))
            .ToList());
    }

    // GET: api/queues/{id}
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var queue = await _repository.GetByIdAsync(id);

        if (queue is null)
            return NotFound();

        var organizationName = await _profiles.GetOrganizationNameAsync(queue.OrganizationId);

        return Ok(QueueResponse.From(queue, organizationName));
    }

    // GET: api/queues/{id}/wait-estimate
    [AllowAnonymous]
    [HttpGet("{id:guid}/wait-estimate")]
    public async Task<IActionResult> GetWaitEstimate(Guid id)
    {
        var queue = await _repository.GetByIdAsync(id);

        if (queue is null)
            return NotFound();

        return Ok(await _repository.GetWaitEstimateAsync(queue));
    }

    // POST: api/queues
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQueueRequest request)
    {
        if (request.OrganizationId == Guid.Empty)
            return BadRequest("A valid organization ID is required.");

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, request.OrganizationId);

        if (forbidden is not null)
            return forbidden;

        var organizationName = await _profiles.GetOrganizationNameAsync(request.OrganizationId);

        if (organizationName is null)
            return BadRequest("Organization was not found.");

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
            QueueResponse.From(created, organizationName));
    }

    // PUT: api/queues/{id}
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQueueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Queue name is required.");

        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return NotFound();

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, existing.OrganizationId);

        if (forbidden is not null)
            return forbidden;

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
    [Authorize]
    [HttpPatch("{id:guid}/open")]
    public async Task<IActionResult> SetOpen(Guid id, [FromBody] SetOpenRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return NotFound();

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, existing.OrganizationId);

        if (forbidden is not null)
            return forbidden;

        var updated = await _repository.SetOpenAsync(id, request.IsOpen);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/queues/{id}
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return NotFound();

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, existing.OrganizationId);

        if (forbidden is not null)
            return forbidden;

        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static string? NameOrNull(IReadOnlyDictionary<Guid, string> names, Guid id)
    {
        return names.TryGetValue(id, out var name) ? name : null;
    }
}
