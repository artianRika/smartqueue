using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/counter-assignments")]
public class CounterAssignmentsController : ControllerBase
{
    private readonly ICounterAssignmentRepository _assignments;
    private readonly ICounterRepository _counters;

    public CounterAssignmentsController(
        ICounterAssignmentRepository assignments,
        ICounterRepository counters)
    {
        _assignments = assignments;
        _counters = counters;
    }

    // GET: api/counters/{counterId}/assignments
    [HttpGet("/api/counters/{counterId:guid}/assignments")]
    public async Task<IActionResult> GetByCounter(Guid counterId)
    {
        var counter = await _counters.GetByIdAsync(counterId);

        if (counter is null)
            return NotFound();

        var assignments = await _assignments.GetByCounterIdAsync(counterId);

        return Ok(assignments);
    }

    // GET: api/counters/{counterId}/assignments/active
    [HttpGet("/api/counters/{counterId:guid}/assignments/active")]
    public async Task<IActionResult> GetActiveByCounter(Guid counterId)
    {
        var counter = await _counters.GetByIdAsync(counterId);

        if (counter is null)
            return NotFound();

        var assignment = await _assignments.GetActiveByCounterIdAsync(counterId);

        if (assignment is null)
            return NotFound();

        return Ok(assignment);
    }

    // GET: api/counter-assignments/active?staffUserId=
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveByStaff([FromQuery] Guid staffUserId)
    {
        if (staffUserId == Guid.Empty)
            return BadRequest("A valid staff user ID is required.");

        var assignment = await _assignments.GetActiveByStaffUserIdAsync(staffUserId);

        if (assignment is null)
            return NotFound();

        return Ok(assignment);
    }

    // GET: api/counter-assignments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var assignment = await _assignments.GetByIdAsync(id);

        if (assignment is null)
            return NotFound();

        return Ok(assignment);
    }

    // POST: api/counters/{counterId}/assignments
    [HttpPost("/api/counters/{counterId:guid}/assignments")]
    public async Task<IActionResult> Create(
        Guid counterId,
        [FromBody] CreateCounterAssignmentRequest request)
    {
        if (request.StaffUserId == Guid.Empty)
            return BadRequest("A valid staff user ID is required.");

        var counter = await _counters.GetByIdAsync(counterId);

        if (counter is null)
            return NotFound();

        var assignment = new CounterAssignment
        {
            Id = Guid.NewGuid(),
            CounterId = counterId,
            StaffUserId = request.StaffUserId,
            AssignedAt = DateTime.UtcNow
        };

        var created = await _assignments.CreateAsync(assignment);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // PATCH: api/counter-assignments/{id}/end
    [HttpPatch("{id:guid}/end")]
    public async Task<IActionResult> End(Guid id)
    {
        var ended = await _assignments.EndAsync(id);

        if (!ended)
            return NotFound();

        return NoContent();
    }
}
