using Microsoft.AspNetCore.Mvc;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/queues/{queueId:guid}/daily-counters")]
public class QueueDailyCountersController : ControllerBase
{
    private readonly IQueueDailyCounterRepository _counters;
    private readonly IQueueRepository _queues;

    public QueueDailyCountersController(
        IQueueDailyCounterRepository counters,
        IQueueRepository queues)
    {
        _counters = counters;
        _queues = queues;
    }

    // GET: api/queues/{queueId}/daily-counters
    [HttpGet]
    public async Task<IActionResult> GetByQueue(Guid queueId)
    {
        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        var counters = await _counters.GetByQueueIdAsync(queueId);

        return Ok(counters);
    }

    // GET: api/queues/{queueId}/daily-counters/{ticketDate}
    [HttpGet("{ticketDate}")]
    public async Task<IActionResult> GetByDate(Guid queueId, DateOnly ticketDate)
    {
        var queue = await _queues.GetByIdAsync(queueId);

        if (queue is null)
            return NotFound();

        var counter = await _counters.GetAsync(queueId, ticketDate);

        if (counter is null)
            return NotFound();

        return Ok(counter);
    }
}
