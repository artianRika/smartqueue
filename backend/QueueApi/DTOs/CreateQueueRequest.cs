using System.ComponentModel.DataAnnotations;

namespace QueueApi.DTOs;

public class CreateQueueRequest
{
    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(10)]
    public string TicketPrefix { get; set; } = string.Empty;
}
