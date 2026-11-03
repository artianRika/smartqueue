using Microsoft.AspNetCore.Mvc;
using ProfileApi.DTOs;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationRepository _repository;

    public OrganizationsController(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Organization>>> GetAll()
    {
        var organizations = await _repository.GetAllAsync();
        return Ok(organizations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Organization>> GetById(Guid id)
    {
        var organization = await _repository.GetByIdAsync(id);

        if (organization == null)
            return NotFound();

        return Ok(organization);
    }

    [HttpPost]
    public async Task<ActionResult<Organization>> Create(
        CreateOrganizationRequest request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Organization name cannot be empty.");

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(organization);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }
}