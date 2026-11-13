using Microsoft.AspNetCore.Mvc;
using ProfileApi.DTOs;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationMembershipsController : ControllerBase
{
    private readonly IOrganizationMembershipRepository _repository;

    public OrganizationMembershipsController(
        IOrganizationMembershipRepository repository)
    {
        _repository = repository;
    }

    // GET: api/OrganizationMemberships
    [HttpGet]
    public async Task<ActionResult<List<OrganizationMembershipResponse>>> GetAll()
    {
        var memberships = await _repository.GetAllAsync();

        return Ok(memberships
            .Select(OrganizationMembershipResponse.From)
            .ToList());
    }

    // GET: api/OrganizationMemberships/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationMembershipResponse>> GetById(
        Guid id)
    {
        var membership = await _repository.GetByIdAsync(id);

        if (membership is null)
            return NotFound();

        return Ok(OrganizationMembershipResponse.From(membership));
    }

    // GET: api/OrganizationMemberships/organization/{organizationId}
    [HttpGet("organization/{organizationId:guid}")]
    public async Task<ActionResult<List<OrganizationMembershipResponse>>> GetByOrganization(
        Guid organizationId)
    {
        var memberships = await _repository.GetByOrganizationAsync(
            organizationId);

        return Ok(memberships
            .Select(OrganizationMembershipResponse.From)
            .ToList());
    }

    // GET: api/OrganizationMemberships/user/{userId}
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<OrganizationMembershipResponse>>> GetByUser(
        Guid userId)
    {
        var memberships = await _repository.GetByUserAsync(userId);

        return Ok(memberships
            .Select(OrganizationMembershipResponse.From)
            .ToList());
    }

    // POST: api/OrganizationMemberships
    [HttpPost]
    public async Task<ActionResult<OrganizationMembershipResponse>> Create(
        CreateMembershipRequest request)
    {
        if (!Enum.IsDefined(request.Role) ||
            request.Role == UserRole.SuperAdmin)
        {
            return BadRequest("Invalid organization role.");
        }

        if (!await _repository.UserExistsAsync(request.UserId))
            return NotFound("User not found.");

        if (!await _repository.OrganizationExistsAsync(request.OrganizationId))
            return NotFound("Organization not found.");

        if (await _repository.MembershipExistsAsync(
                request.UserId,
                request.OrganizationId))
        {
            return Conflict("User already belongs to this organization.");
        }

        var membership = new OrganizationMembership
        {
            UserId = request.UserId,
            OrganizationId = request.OrganizationId,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(membership);

        var created = await _repository.GetByIdAsync(membership.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = membership.Id },
            OrganizationMembershipResponse.From(created!)
        );
    }

    // PUT: api/OrganizationMemberships/{id}/role
    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        UpdateMembershipRoleRequest request)
    {
        if (!Enum.IsDefined(request.Role) ||
            request.Role == UserRole.SuperAdmin)
        {
            return BadRequest("Invalid organization role.");
        }

        var updated = await _repository.UpdateRoleAsync(
            id,
            request.Role);

        if (!updated)
            return NotFound("Membership not found.");

        return NoContent();
    }

    // DELETE: api/OrganizationMemberships/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return NotFound("Membership not found.");

        return NoContent();
    }
}