using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileApi.DTOs;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrganizationMembershipsController : ControllerBase
{
    private readonly IOrganizationMembershipRepository _repository;
    private readonly IUserProfileRepository _userRepository;

    public OrganizationMembershipsController(
        IOrganizationMembershipRepository repository,
        IUserProfileRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
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
            await _repository.GetByUserAndOrganizationAsync(
                user.Id,
                organizationId);

        return membership?.Role == UserRole.OrganizationAdmin;
    }

    // GET: api/OrganizationMemberships
    // SuperAdmin only
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

        var memberships = await _repository.GetAllAsync();

        return Ok(memberships
            .Select(OrganizationMembershipResponse.From)
            .ToList());
    }

    // GET: api/OrganizationMemberships/{id}
    // SuperAdmin, organization admin, or membership owner
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();

        if (currentUser is null)
            return Forbid();

        var membership = await _repository.GetByIdAsync(id);

        if (membership is null)
            return NotFound();

        var isOwner = membership.UserId == currentUser.Id;

        if (!isOwner &&
            !await CanManageOrganizationAsync(membership.OrganizationId))
        {
            return Forbid();
        }

        return Ok(OrganizationMembershipResponse.From(membership));
    }

    // GET: api/OrganizationMemberships/organization/{organizationId}
    // SuperAdmin or OrganizationAdmin of that organization
    [HttpGet("organization/{organizationId:guid}")]
    public async Task<IActionResult> GetByOrganization(Guid organizationId)
    {
        if (!await CanManageOrganizationAsync(organizationId))
            return Forbid();

        var memberships =
            await _repository.GetByOrganizationAsync(organizationId);

        return Ok(memberships
            .Select(OrganizationMembershipResponse.From)
            .ToList());
    }

    // GET: api/OrganizationMemberships/user/{userId}
    // SuperAdmin or user themselves
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var currentUser = await GetCurrentUserAsync();

        if (currentUser is null)
            return Forbid();

        if (currentUser.Id != userId &&
            currentUser.Role != UserRole.SuperAdmin)
        {
            return Forbid();
        }

        var memberships = await _repository.GetByUserAsync(userId);

        return Ok(memberships
            .Select(OrganizationMembershipResponse.From)
            .ToList());
    }

    // POST: api/OrganizationMemberships
    // OrganizationAdmin can add Staff to own organization
    // SuperAdmin can assign any organization role except SuperAdmin
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMembershipRequest request)
    {
        if (!Enum.IsDefined(request.Role) ||
            request.Role == UserRole.SuperAdmin)
        {
            return BadRequest("Invalid organization role.");
        }

        var currentUser = await GetCurrentUserAsync();

        if (currentUser is null)
            return Forbid();

        var isSuperAdmin = currentUser.Role == UserRole.SuperAdmin;

        if (!isSuperAdmin)
        {
            if (request.Role != UserRole.Staff)
                return Forbid();

            if (!await CanManageOrganizationAsync(request.OrganizationId))
                return Forbid();
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
    // SuperAdmin only
    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateMembershipRoleRequest request)
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

        if (!Enum.IsDefined(request.Role) ||
            request.Role == UserRole.SuperAdmin)
        {
            return BadRequest("Invalid organization role.");
        }

        var updated = await _repository.UpdateRoleAsync(
            id,
            request.Role);

        return updated ? NoContent() : NotFound();
    }

    // DELETE: api/OrganizationMemberships/{id}
    // SuperAdmin can delete any membership
    // OrganizationAdmin can remove Staff from own organization
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();

        if (currentUser is null)
            return Forbid();

        var membership = await _repository.GetByIdAsync(id);

        if (membership is null)
            return NotFound();

        var isSuperAdmin = currentUser.Role == UserRole.SuperAdmin;

        if (!isSuperAdmin)
        {
            if (membership.Role != UserRole.Staff)
                return Forbid();

            if (!await CanManageOrganizationAsync(membership.OrganizationId))
                return Forbid();
        }

        var deleted = await _repository.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}