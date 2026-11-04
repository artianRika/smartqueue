using QueueApi.Models;

namespace QueueApi.DTOs;

public class TicketResponse
{
    public Guid Id { get; set; }

    public Guid QueueId { get; set; }

    public Guid UserId { get; set; }

    public string? UserName { get; set; }

    public Guid? CounterId { get; set; }

    public int Number { get; set; }

    public string TicketCode { get; set; } = string.Empty;

    public DateOnly TicketDate { get; set; }

    public TicketStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CalledAt { get; set; }

    public DateTime? ServiceStartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public static TicketResponse From(Ticket ticket, string? userName)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            QueueId = ticket.QueueId,
            UserId = ticket.UserId,
            UserName = userName,
            CounterId = ticket.CounterId,
            Number = ticket.Number,
            TicketCode = ticket.TicketCode,
            TicketDate = ticket.TicketDate,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            CalledAt = ticket.CalledAt,
            ServiceStartedAt = ticket.ServiceStartedAt,
            CompletedAt = ticket.CompletedAt
        };
    }
}
