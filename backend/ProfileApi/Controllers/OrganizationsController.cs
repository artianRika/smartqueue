using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileApi.DTOs;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Controllers;

[ApiController]
[Authorize]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationRepository _repository;
    private readonly IUserProfileRepository _userRepository;
    private readonly IOrganizationMembershipRepository _membershipRepository;

    public OrganizationsController(
        IOrganizationRepository repository,
        IUserProfileRepository userRepository,
        IOrganizationMembershipRepository membershipRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
    }

    private string? ClerkUserId =>
        User.FindFirstValue("sub");

    private async Task<UserProfile?> GetCurrentUserAsync()
    {
        if (string.IsNullOrWhiteSpace(ClerkUserId))
            return null;

        return await _userRepository.GetByClerkUserIdAsync(ClerkUserId);
    }

    private async Task<bool> IsSuperAdminAsync()
    {
        var user = await GetCurrentUserAsync();

        return user?.Role == UserRole.SuperAdmin;
    }

    private async Task<bool> CanManageOrganizationAsync(Guid organizationId)
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
            return false;

        if (user.Role == UserRole.SuperAdmin)
            return true;

        var membership =
            await _membershipRepository.GetByUserAndOrganizationAsync(
                user.Id,
                organizationId);

        return membership?.Role == UserRole.OrganizationAdmin;
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

        return organization is null
            ? NotFound()
            : Ok(organization);
    }

    // POST: api/organizations
    // SuperAdmin only
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrganizationRequest request)
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

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
    // SuperAdmin or OrganizationAdmin of this organization
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateOrganizationRequest request)
    {
        if (!await CanManageOrganizationAsync(id))
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Organization name is required.");

        var organization = new Organization
        {
            Id = id,
            Name = request.Name.Trim()
        };

        var updated = await _repository.UpdateAsync(organization);

        return updated ? NoContent() : NotFound();
    }

    // DELETE: api/organizations/{id}
    // SuperAdmin only
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

        var deleted = await _repository.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}