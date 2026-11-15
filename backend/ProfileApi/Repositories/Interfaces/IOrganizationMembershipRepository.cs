using ProfileApi.Models;

namespace ProfileApi.Repositories.Interfaces;

public interface IOrganizationMembershipRepository
{
    Task<List<OrganizationMembership>> GetAllAsync();

    Task<OrganizationMembership?> GetByIdAsync(Guid id);

    Task<List<OrganizationMembership>> GetByOrganizationAsync(
        Guid organizationId);

    Task<List<OrganizationMembership>> GetByUserAsync(
        Guid userId);
    
    Task<OrganizationMembership?> GetByUserAndOrganizationAsync(
        Guid userId,
        Guid organizationId);

    Task<bool> UserExistsAsync(Guid userId);

    Task<bool> OrganizationExistsAsync(Guid organizationId);

    Task<bool> MembershipExistsAsync(
        Guid userId,
        Guid organizationId);

    Task<OrganizationMembership> CreateAsync(
        OrganizationMembership membership);

    Task<bool> UpdateRoleAsync(Guid id, UserRole role);

    Task<bool> DeleteAsync(Guid id);
}