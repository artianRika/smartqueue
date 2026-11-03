using Microsoft.EntityFrameworkCore;
using ProfileApi.Data;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly ProfileDbContext _context;

    public OrganizationRepository(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<List<Organization>> GetAllAsync()
    {
        return await _context.Organizations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Organization?> GetByIdAsync(Guid id)
    {
        return await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Organization> CreateAsync(Organization organization)
    {
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        return organization;
    }
}