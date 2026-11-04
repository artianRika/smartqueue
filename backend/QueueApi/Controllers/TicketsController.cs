using Microsoft.AspNetCore.Mvc;
using QueueApi.DTOs;
using QueueApi.Infrastructure;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository _tickets;
    private readonly IQueueRepository _queues;
    private readonly ICounterRepository _counters;

    public TicketsController(
        ITicketRepository tickets,
        IQueueRepository queues,
        ICounterRepository counters)
    {
        _tickets = tickets;
        _queues = queues;
        _counters = counters;
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

        return Ok(tickets);
    }

    // GET: api/tickets/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await _tickets.GetByIdAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(ticket);
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

        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        if (!queue.IsOpen)
            throw new ConflictException("Queue is closed.");

        var created = await _tickets.CreateAsync(queueId, request.UserId, queue.TicketPrefix);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
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

        return Ok(ticket);
    }

    // PATCH: api/tickets/{id}/start
    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var ticket = await _tickets.StartServiceAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(ticket);
    }

    // PATCH: api/tickets/{id}/complete
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ticket = await _tickets.CompleteAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(ticket);
    }

    // PATCH: api/tickets/{id}/cancel
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ticket = await _tickets.CancelAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(ticket);
    }

    // PATCH: api/tickets/{id}/no-show
    [HttpPatch("{id:guid}/no-show")]
    public async Task<IActionResult> MarkNoShow(Guid id)
    {
        var ticket = await _tickets.MarkNoShowAsync(id);

        if (ticket is null)
            return NotFound();

        return Ok(ticket);
    }
}
