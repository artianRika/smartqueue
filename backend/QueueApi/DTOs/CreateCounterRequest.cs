using System.ComponentModel.DataAnnotations;

namespace QueueApi.DTOs;

public class CreateCounterRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
