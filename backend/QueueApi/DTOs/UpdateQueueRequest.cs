using System.ComponentModel.DataAnnotations;

namespace QueueApi.DTOs;

public class UpdateQueueRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}
