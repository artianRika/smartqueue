using System.ComponentModel.DataAnnotations;

namespace QueueApi.DTOs;

public class CreateCounterAssignmentRequest
{
    [Required]
    public Guid StaffUserId { get; set; }
}
