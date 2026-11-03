using ProfileApi.Models;

namespace ProfileApi.Repositories.Interfaces;

public interface IUserProfileRepository
{
    Task<List<UserProfile>> GetAllAsync();
    Task<UserProfile?> GetByIdAsync(Guid id);
    Task<UserProfile> CreateAsync(UserProfile profile);
    Task<bool> UpdateAsync(UserProfile profile);
}