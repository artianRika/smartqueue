using ProfileApi.Models;

namespace ProfileApi.DTOs;

public record OrganizationMembershipResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    Guid OrganizationId,
    string OrganizationName,
    UserRole Role,
    DateTime CreatedAt
)
{
    public static OrganizationMembershipResponse From(
        OrganizationMembership membership)
    {
        return new OrganizationMembershipResponse(
            membership.Id,
            membership.UserId,
            membership.User.FirstName,
            membership.User.LastName,
            membership.User.Email,
            membership.OrganizationId,
            membership.Organization.Name,
            membership.Role,
            membership.CreatedAt
        );
    }
}