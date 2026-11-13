using ProfileApi.Models;

namespace ProfileApi.DTOs;

public record UpdateMembershipRoleRequest(
    UserRole Role
);