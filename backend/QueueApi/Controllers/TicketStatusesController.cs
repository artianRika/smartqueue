using Microsoft.AspNetCore.Mvc;
using QueueApi.Models;

namespace QueueApi.Controllers;

[ApiController]
[Route("api/ticket-statuses")]
public class TicketStatusesController : ControllerBase
{
    // GET: api/ticket-statuses
    [HttpGet]
    public IActionResult GetAll()
    {
        var statuses = Enum.GetNames<TicketStatus>();

        return Ok(statuses);
    }
}
