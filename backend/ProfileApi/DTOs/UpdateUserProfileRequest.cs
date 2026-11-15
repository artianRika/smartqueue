using System.ComponentModel.DataAnnotations;

namespace ProfileApi.DTOs;

public class UpdateUserProfileRequest
{
    [Required]
    [MaxLength(200)]
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
}