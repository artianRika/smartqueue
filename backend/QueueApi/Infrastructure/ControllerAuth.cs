using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QueueApi.Services;

namespace QueueApi.Infrastructure;

public static class ControllerAuth
{
    public static string? GetClerkUserId(this ControllerBase controller) =>
        controller.User.FindFirstValue("sub");

    public static async Task<ActionResult<ProfileUser>> RequireProfileAsync(
        this ControllerBase controller,
        IProfileLookup profiles)
    {
        var clerkUserId = controller.GetClerkUserId();

        if (string.IsNullOrWhiteSpace(clerkUserId))
            return controller.Unauthorized();

        var profile = await profiles.GetUserByClerkIdAsync(clerkUserId);

        if (profile is null)
            return controller.NotFound("Profile not created yet. Call ProfileApi POST /api/userprofiles/me first.");

        return profile;
    }

    public static async Task<IActionResult?> RequireStaffForOrganizationAsync(
        this ControllerBase controller,
        IProfileLookup profiles,
        Guid organizationId)
    {
        var profileResult = await controller.RequireProfileAsync(profiles);

        if (profileResult.Result is not null)
            return profileResult.Result;

        var profile = profileResult.Value!;
        var role = await profiles.GetOrganizationRoleAsync(profile.Id, organizationId);

        if (!OrganizationRoles.CanStaffQueue(role))
            return controller.Forbid();

        return null;
    }
}
