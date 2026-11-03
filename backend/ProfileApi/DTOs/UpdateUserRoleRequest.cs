using System.ComponentModel.DataAnnotations;
using ProfileApi.Models;

namespace ProfileApi.DTOs;

public class UpdateUserRoleRequest
{
    [Required]
    public UserRole? Role { get; set; }
}