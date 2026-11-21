using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueApi.Messaging;

namespace QueueApi.Controllers;

[ApiController]
[Authorize]
[Route("api/signalr")]
public class SignalRController : ControllerBase
{
    private readonly SignalRNegotiateService? _signalR;

    public SignalRController(IEnumerable<SignalRNegotiateService> signalR)
    {
        _signalR = signalR.FirstOrDefault();
    }

    // POST: api/signalr/negotiate?ticketId=&queueId=
    [HttpPost("negotiate")]
    public async Task<IActionResult> Negotiate(
        [FromQuery] Guid? ticketId,
        [FromQuery] Guid? queueId,
        CancellationToken cancellationToken)
    {
        if (_signalR is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "SignalR is not configured.");

        var userId = User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var response = await _signalR.NegotiateAsync(
            userId,
            ticketId,
            queueId,
            cancellationToken);

        return Ok(response);
    }
}
