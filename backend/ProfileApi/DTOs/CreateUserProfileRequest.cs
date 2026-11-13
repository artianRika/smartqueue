using System.ComponentModel.DataAnnotations;
using ProfileApi.Models;

namespace ProfileApi.DTOs;

public class CreateUserProfileRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string LastName { get; set; } = string.Empty;
    
    

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public UserRole? Role { get; set; }
}