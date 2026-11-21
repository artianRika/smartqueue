using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Infrastructure;
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
    private readonly IQueueRepository _queues;
    private readonly IProfileLookup _profiles;

    public CounterAssignmentsController(
        ICounterAssignmentRepository assignments,
        ICounterRepository counters,
        IQueueRepository queues,
        IProfileLookup profiles)
    {
        _assignments = assignments;
        _counters = counters;
        _queues = queues;
        _profiles = profiles;
    }

    // GET: api/counters/{counterId}/assignments
    [AllowAnonymous]
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
    [AllowAnonymous]
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
    [AllowAnonymous]
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
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var assignment = await _assignments.GetByIdAsync(id);

        if (assignment is null)
            return NotFound();

        return Ok(await MapAssignmentAsync(assignment));
    }

    // POST: api/counters/{counterId}/assignments
    [Authorize]
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

        var queue = await _queues.GetByIdAsync(counter.QueueId);

        if (queue is null)
            return NotFound();

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, queue.OrganizationId);

        if (forbidden is not null)
            return forbidden;

        var staff = await _profiles.GetUserAsync(request.StaffUserId);

        if (staff is null)
            return BadRequest("Staff user was not found.");

        var staffRole = await _profiles.GetOrganizationRoleAsync(
            request.StaffUserId,
            queue.OrganizationId);

        if (!OrganizationRoles.CanStaffQueue(staffRole))
            return BadRequest("Only Staff, OrganizationAdmin, or SuperAdmin can be assigned to a counter.");

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
            CounterAssignmentResponse.From(created, $"{staff.FirstName} {staff.LastName}".Trim()));
    }

    // PATCH: api/counter-assignments/{id}/end
    [Authorize]
    [HttpPatch("{id:guid}/end")]
    public async Task<IActionResult> End(Guid id)
    {
        var assignment = await _assignments.GetByIdAsync(id);

        if (assignment is null)
            return NotFound();

        var counter = await _counters.GetByIdAsync(assignment.CounterId);

        if (counter is null)
            return NotFound();

        var queue = await _queues.GetByIdAsync(counter.QueueId);

        if (queue is null)
            return NotFound();

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, queue.OrganizationId);

        if (forbidden is not null)
            return forbidden;

        var ended = await _assignments.EndAsync(id);

        if (!ended)
            return NotFound();

        return NoContent();
    }

    private async Task<CounterAssignmentResponse> MapAssignmentAsync(CounterAssignment assignment)
    {
        var staff = await _profiles.GetUserAsync(assignment.StaffUserId);

        var name = staff is null ? null : $"{staff.FirstName} {staff.LastName}".Trim();

        return CounterAssignmentResponse.From(assignment, name);
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
