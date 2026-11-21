using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Infrastructure;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;
using QueueApi.Services;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository _tickets;
    private readonly IQueueRepository _queues;
    private readonly ICounterRepository _counters;
    private readonly IProfileLookup _profiles;

    public TicketsController(
        ITicketRepository tickets,
        IQueueRepository queues,
        ICounterRepository counters,
        IProfileLookup profiles)
    {
        _tickets = tickets;
        _queues = queues;
        _counters = counters;
        _profiles = profiles;
    }

    // GET: api/queues/{queueId}/tickets?status=&ticketDate=
    [AllowAnonymous]
    [HttpGet("/api/queues/{queueId:guid}/tickets")]
    public async Task<IActionResult> GetByQueue(
        Guid queueId,
        [FromQuery] TicketStatus? status,
        [FromQuery] DateOnly? ticketDate)
    {
        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        var tickets = await _tickets.GetByQueueAsync(queueId, status, ticketDate);

        return Ok(await MapTicketsAsync(tickets));
    }

    // GET: api/tickets/{id}
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await _tickets.GetByIdAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(await MapTicketAsync(ticket));
    }

    // GET: api/tickets/{id}/position
    [AllowAnonymous]
    [HttpGet("{id:guid}/position")]
    public async Task<IActionResult> GetPosition(Guid id)
    {
        var position = await _tickets.GetPositionAsync(id);

        if (position is null)
            return NotFound();

        return Ok(position);
    }

    // POST: api/queues/{queueId}/tickets
    [Authorize]
    [HttpPost("/api/queues/{queueId:guid}/tickets")]
    public async Task<IActionResult> Create(Guid queueId)
    {
        var profileResult = await this.RequireProfileAsync(_profiles);

        if (profileResult.Result is not null)
            return profileResult.Result;

        var user = profileResult.Value!;

        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        if (!queue.IsOpen)
            throw new ConflictException("Queue is closed.");

        var created = await _tickets.CreateAsync(queueId, user.Id, queue.TicketPrefix);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            TicketResponse.From(created, $"{user.FirstName} {user.LastName}".Trim()));
    }

    // POST: api/counters/{counterId}/tickets/call-next
    [Authorize]
    [HttpPost("/api/counters/{counterId:guid}/tickets/call-next")]
    public async Task<IActionResult> CallNext(Guid counterId)
    {
        var counter = await _counters.GetByIdAsync(counterId);

        if (counter is null)
            return NotFound();

        var queue = await _queues.GetByIdAsync(counter.QueueId);

        if (queue is null)
            return NotFound();

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, queue.OrganizationId);

        if (forbidden is not null)
            return forbidden;

        if (!counter.IsOpen)
            throw new ConflictException("Counter is closed.");

        if (!queue.IsOpen)
            throw new ConflictException("Queue is closed.");

        var ticket = await _tickets.CallNextAsync(counter.Id, counter.QueueId);

        if (ticket is null)
            return NotFound("No waiting tickets.");

        return Ok(await MapTicketAsync(ticket));
    }

    // PATCH: api/tickets/{id}/start
    [Authorize]
    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        return await TransitionForStaffAsync(id, () => _tickets.StartServiceAsync(id));
    }

    // PATCH: api/tickets/{id}/complete
    [Authorize]
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        return await TransitionForStaffAsync(id, () => _tickets.CompleteAsync(id));
    }

    // PATCH: api/tickets/{id}/cancel
    [Authorize]
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return await TransitionForStaffAsync(id, () => _tickets.CancelAsync(id));
    }

    // PATCH: api/tickets/{id}/no-show
    [Authorize]
    [HttpPatch("{id:guid}/no-show")]
    public async Task<IActionResult> MarkNoShow(Guid id)
    {
        return await TransitionForStaffAsync(id, () => _tickets.MarkNoShowAsync(id));
    }

    private async Task<IActionResult> TransitionForStaffAsync(
        Guid ticketId,
        Func<Task<Ticket?>> transition)
    {
        var existing = await _tickets.GetByIdAsync(ticketId);

        if (existing is null)
            return NotFound();

        var queue = await _queues.GetByIdAsync(existing.QueueId);

        if (queue is null)
            return NotFound();

        var forbidden = await this.RequireStaffForOrganizationAsync(_profiles, queue.OrganizationId);

        if (forbidden is not null)
            return forbidden;

        var ticket = await transition();

        if (ticket is null)
            return NotFound();

        return Ok(await MapTicketAsync(ticket));
    }

    private async Task<TicketResponse> MapTicketAsync(Ticket ticket)
    {
        var user = await _profiles.GetUserAsync(ticket.UserId);

        var name = user is null ? null : $"{user.FirstName} {user.LastName}".Trim();

        return TicketResponse.From(ticket, name);
    }

    private async Task<List<TicketResponse>> MapTicketsAsync(List<Ticket> tickets)
    {
        var names = await _profiles.GetUserNamesAsync(tickets.Select(ticket => ticket.UserId));

        return tickets
            .Select(ticket => TicketResponse.From(
                ticket,
                names.TryGetValue(ticket.UserId, out var name) ? name : null))
            .ToList();
    }
}
