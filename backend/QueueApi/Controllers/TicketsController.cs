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
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await _tickets.GetByIdAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(await MapTicketAsync(ticket));
    }

    // GET: api/tickets/{id}/position
    [HttpGet("{id:guid}/position")]
    public async Task<IActionResult> GetPosition(Guid id)
    {
        var position = await _tickets.GetPositionAsync(id);

        if (position is null)
            return NotFound();

        return Ok(position);
    }

    // POST: api/queues/{queueId}/tickets
    [HttpPost("/api/queues/{queueId:guid}/tickets")]
    public async Task<IActionResult> Create(Guid queueId, [FromBody] CreateTicketRequest request)
    {
        if (request.UserId == Guid.Empty)
            return BadRequest("A valid user ID is required.");

        var user = await _profiles.GetUserAsync(request.UserId);

        if (user is null)
            return BadRequest("User was not found.");

        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        if (!queue.IsOpen)
            throw new ConflictException("Queue is closed.");

        var created = await _tickets.CreateAsync(queueId, request.UserId, queue.TicketPrefix);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            TicketResponse.From(created, user.FullName));
    }

    // POST: api/counters/{counterId}/tickets/call-next
    [HttpPost("/api/counters/{counterId:guid}/tickets/call-next")]
    public async Task<IActionResult> CallNext(Guid counterId)
    {
        var counter = await _counters.GetByIdAsync(counterId);

        if (counter is null)
            return NotFound();

        if (!counter.IsOpen)
            throw new ConflictException("Counter is closed.");

        var queue = await _queues.GetByIdAsync(counter.QueueId);

        if (queue is null)
            return NotFound();

        if (!queue.IsOpen)
            throw new ConflictException("Queue is closed.");

        var ticket = await _tickets.CallNextAsync(counter.Id, counter.QueueId);

        if (ticket is null)
            return NotFound("No waiting tickets.");

        return Ok(await MapTicketAsync(ticket));
    }

    // PATCH: api/tickets/{id}/start
    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var ticket = await _tickets.StartServiceAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(await MapTicketAsync(ticket));
    }

    // PATCH: api/tickets/{id}/complete
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ticket = await _tickets.CompleteAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(await MapTicketAsync(ticket));
    }

    // PATCH: api/tickets/{id}/cancel
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ticket = await _tickets.CancelAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(await MapTicketAsync(ticket));
    }

    // PATCH: api/tickets/{id}/no-show
    [HttpPatch("{id:guid}/no-show")]
    public async Task<IActionResult> MarkNoShow(Guid id)
    {
        var ticket = await _tickets.MarkNoShowAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(await MapTicketAsync(ticket));
    }

    private async Task<TicketResponse> MapTicketAsync(Ticket ticket)
    {
        var user = await _profiles.GetUserAsync(ticket.UserId);

        return TicketResponse.From(ticket, user?.FullName);
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
