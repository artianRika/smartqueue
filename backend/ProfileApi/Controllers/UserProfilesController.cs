using Microsoft.AspNetCore.Mvc;
using ProfileApi.DTOs;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Controllers;

[ApiController]
[Route("api/userprofiles")]
public class UserProfilesController : ControllerBase
{
    private readonly IUserProfileRepository _repository;

    public UserProfilesController(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await _repository.GetAllAsync();

        return Ok(profiles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _repository.GetByIdAsync(id);

        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserProfileRequest request)
    {
        if (request.Id == Guid.Empty)
            return BadRequest("A valid user ID is required.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("Full name is required.");

        var existing = await _repository.GetByIdAsync(request.Id);

        if (existing is not null)
            return Conflict("A user profile with this ID already exists.");
        
        if (request.Role is null || !Enum.IsDefined(request.Role.Value))
            return BadRequest("A valid role is required.");

        var profile = new UserProfile
        {
            Id = request.Id,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Role = request.Role.Value,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(profile);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("Full name is required.");

        var profile = new UserProfile
        {
            Id = id,
            FullName = request.FullName.Trim()
        };

        var updated = await _repository.UpdateAsync(profile);

        if (!updated)
            return NotFound();

        return NoContent();
    }
}