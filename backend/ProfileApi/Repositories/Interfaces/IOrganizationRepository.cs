using ProfileApi.Models;

namespace ProfileApi.Repositories.Interfaces;

public interface IOrganizationRepository
{
    Task<List<Organization>> GetAllAsync();
    Task<Organization?> GetByIdAsync(Guid id);
    Task<Organization> CreateAsync(Organization organization);
}