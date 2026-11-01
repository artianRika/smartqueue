using System.ComponentModel.DataAnnotations;

namespace ProfileApi.DTOs;

public class UpdateUserProfileRequest
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
}