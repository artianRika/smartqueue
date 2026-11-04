using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;
using QueueApi.Services;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/counter-assignments")]
public class CounterAssignmentsController : ControllerBase
{
    private readonly ICounterAssignmentRepository _assignments;
    private readonly ICounterRepository _counters;
    private readonly IProfileLookup _profiles;

    public CounterAssignmentsController(
        ICounterAssignmentRepository assignments,
        ICounterRepository counters,
        IProfileLookup profiles)
    {
        _assignments = assignments;
        _counters = counters;
        _profiles = profiles;
    }

    // GET: api/counters/{counterId}/assignments
    [HttpGet("/api/counters/{counterId:guid}/assignments")]
    public async Task<IActionResult> GetByCounter(Guid counterId)
    {
        var counter = await _counters.GetByIdAsync(counterId);

        if (counter is null)
            return NotFound();

        var assignments = await _assignments.GetByCounterIdAsync(counterId);

        return Ok(await MapAssignmentsAsync(assignments));
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

        return Ok(await MapAssignmentAsync(assignment));
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

        return Ok(await MapAssignmentAsync(assignment));
    }

    // GET: api/counter-assignments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var assignment = await _assignments.GetByIdAsync(id);

        if (assignment is null)
            return NotFound();

        return Ok(await MapAssignmentAsync(assignment));
    }

    // POST: api/counters/{counterId}/assignments
    [HttpPost("/api/counters/{counterId:guid}/assignments")]
    public async Task<IActionResult> Create(
        Guid counterId,
        [FromBody] CreateCounterAssignmentRequest request)
    {
        if (request.StaffUserId == Guid.Empty)
            return BadRequest("A valid staff user ID is required.");

        var staff = await _profiles.GetUserAsync(request.StaffUserId);

        if (staff is null)
            return BadRequest("Staff user was not found.");

        if (staff.Role is not ("Staff" or "Admin"))
            return BadRequest("Only a staff or admin user can be assigned to a counter.");

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
            CounterAssignmentResponse.From(created, staff.FullName));
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

    private async Task<CounterAssignmentResponse> MapAssignmentAsync(CounterAssignment assignment)
    {
        var staff = await _profiles.GetUserAsync(assignment.StaffUserId);

        return CounterAssignmentResponse.From(assignment, staff?.FullName);
    }

    private async Task<List<CounterAssignmentResponse>> MapAssignmentsAsync(
        List<CounterAssignment> assignments)
    {
        var names = await _profiles.GetUserNamesAsync(assignments.Select(assignment => assignment.StaffUserId));

        return assignments
            .Select(assignment => CounterAssignmentResponse.From(
                assignment,
                names.TryGetValue(assignment.StaffUserId, out var name) ? name : null))
            .ToList();
    }
}
