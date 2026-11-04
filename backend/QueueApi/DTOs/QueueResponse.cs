using QueueApi.Models;

namespace QueueApi.DTOs;

public class QueueResponse
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string? OrganizationName { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string TicketPrefix { get; set; } = string.Empty;

    public bool IsOpen { get; set; }

    public DateTime CreatedAt { get; set; }

    public static QueueResponse From(Queue queue, string? organizationName)
    {
        return new QueueResponse
        {
            Id = queue.Id,
            OrganizationId = queue.OrganizationId,
            OrganizationName = organizationName,
            Name = queue.Name,
            Description = queue.Description,
            TicketPrefix = queue.TicketPrefix,
            IsOpen = queue.IsOpen,
            CreatedAt = queue.CreatedAt
        };
    }
}
