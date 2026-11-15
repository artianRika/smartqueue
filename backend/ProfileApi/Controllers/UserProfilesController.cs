using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileApi.DTOs;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Controllers;

[ApiController]
[Authorize]
[Route("api/userprofiles")]
public class UserProfilesController : ControllerBase
{
    private readonly IUserProfileRepository _repository;

    public UserProfilesController(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    private string? ClerkUserId =>
        User.FindFirstValue("sub");

    private async Task<bool> IsSuperAdminAsync()
    {
        if (string.IsNullOrWhiteSpace(ClerkUserId))
            return false;

        var user = await _repository.GetByClerkUserIdAsync(ClerkUserId);

        return user?.Role == UserRole.SuperAdmin;
    }

    // GET: api/userprofiles/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        if (string.IsNullOrWhiteSpace(ClerkUserId))
            return Unauthorized();

        var profile = await _repository.GetByClerkUserIdAsync(
            ClerkUserId);

        if (profile is null)
            return NotFound("Profile not created yet.");

        return Ok(profile);
    }

    // POST: api/userprofiles/me
    [HttpPost("me")]
    public async Task<IActionResult> CreateMe(
        [FromBody] CreateUserProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(ClerkUserId))
            return Unauthorized();

        var existing = await _repository.GetByClerkUserIdAsync(
            ClerkUserId);

        if (existing is not null)
            return Conflict("Profile already exists.");

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("First name, last name and email are required.");
        }
        

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            ClerkUserId = ClerkUserId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Role = UserRole.Visitor,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(profile);

        return CreatedAtAction(
            nameof(GetMe),
            null,
            created);
    }

    // PUT: api/userprofiles/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateUserProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(ClerkUserId))
            return Unauthorized();

        var existing = await _repository.GetByClerkUserIdAsync(
            ClerkUserId);

        if (existing is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest("First and last name are required.");
        }

        var profile = new UserProfile
        {
            Id = existing.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        var updated = await _repository.UpdateAsync(profile);

        return updated ? NoContent() : NotFound();
    }

    // GET: api/userprofiles
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

        return Ok(await _repository.GetAllAsync());
    }

    // GET: api/userprofiles/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

        var profile = await _repository.GetByIdAsync(id);

        return profile is null ? NotFound() : Ok(profile);
    }

    // PATCH: api/userprofiles/{id}/role
    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request)
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

        if (request.Role is null ||
            !Enum.IsDefined(request.Role.Value))
        {
            return BadRequest("Invalid role.");
        }

        // Organization-specific roles belong to memberships.
        if (request.Role.Value is not
            (UserRole.Visitor or UserRole.SuperAdmin))
        {
            return BadRequest(
                "Staff and OrganizationAdmin must be assigned through organization memberships.");
        }

        var updated = await _repository.UpdateRoleAsync(
            id,
            request.Role.Value);

        return updated ? NoContent() : NotFound();
    }

    // DELETE: api/userprofiles/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await IsSuperAdminAsync())
            return Forbid();

        var deleted = await _repository.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}