using ProfileApi.Models;

namespace ProfileApi.DTOs;

public record CreateMembershipRequest(
    Guid UserId,
    Guid OrganizationId,
    UserRole Role
);