using System.ComponentModel.DataAnnotations;

namespace QueueApi.DTOs;

public class CreateTicketRequest
{
    [Required]
    public Guid UserId { get; set; }
}
