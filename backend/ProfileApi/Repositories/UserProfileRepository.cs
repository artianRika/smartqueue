using Microsoft.EntityFrameworkCore;
using ProfileApi.Data;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly ProfileDbContext _context;

    public UserProfileRepository(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserProfile>> GetAllAsync()
    {
        return await _context.UserProfiles
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        return await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<UserProfile> CreateAsync(UserProfile profile)
    {
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    public async Task<bool> UpdateAsync(UserProfile profile)
    {
        var existing = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.Id == profile.Id);

        if (existing is null)
            return false;

        existing.FullName = profile.FullName;

        await _context.SaveChangesAsync();

        return true;
    }
}