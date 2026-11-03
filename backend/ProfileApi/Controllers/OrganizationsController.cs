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

    // GET: api/organizations
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var organizations = await _repository.GetAllAsync();

        return Ok(organizations);
    }

    // GET: api/organizations/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var organization = await _repository.GetByIdAsync(id);

        if (organization is null)
            return NotFound();

        return Ok(organization);
    }

    // POST: api/organizations
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrganizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Organization name is required.");

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(organization);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // PUT: api/organizations/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateOrganizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Organization name is required.");

        var organization = new Organization
        {
            Id = id,
            Name = request.Name.Trim()
        };

        var updated = await _repository.UpdateAsync(organization);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/organizations/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}