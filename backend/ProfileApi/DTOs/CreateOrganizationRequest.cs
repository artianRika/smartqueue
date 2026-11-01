using System.ComponentModel.DataAnnotations;

namespace ProfileApi.DTOs;

public class CreateOrganizationRequest

{

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

}