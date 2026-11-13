using Microsoft.EntityFrameworkCore;
using ProfileApi.Data;
using ProfileApi.Models;
using ProfileApi.Repositories.Interfaces;

namespace ProfileApi.Repositories;

public class OrganizationMembershipRepository
    : IOrganizationMembershipRepository
{
    private readonly ProfileDbContext _context;

    public OrganizationMembershipRepository(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationMembership>> GetAllAsync()
    {
        return await _context.OrganizationMemberships
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Organization)
            .ToListAsync();
    }

    public async Task<OrganizationMembership?> GetByIdAsync(Guid id)
    {
        return await _context.OrganizationMemberships
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<OrganizationMembership>> GetByOrganizationAsync(
        Guid organizationId)
    {
        return await _context.OrganizationMemberships
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Organization)
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<List<OrganizationMembership>> GetByUserAsync(
        Guid userId)
    {
        return await _context.OrganizationMemberships
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Organization)
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _context.UserProfiles
            .AnyAsync(x => x.Id == userId);
    }

    public async Task<bool> OrganizationExistsAsync(Guid organizationId)
    {
        return await _context.Organizations
            .AnyAsync(x => x.Id == organizationId);
    }

    public async Task<bool> MembershipExistsAsync(
        Guid userId,
        Guid organizationId)
    {
        return await _context.OrganizationMemberships
            .AnyAsync(x =>
                x.UserId == userId &&
                x.OrganizationId == organizationId);
    }

    public async Task<OrganizationMembership> CreateAsync(
        OrganizationMembership membership)
    {
        _context.OrganizationMemberships.Add(membership);

        await _context.SaveChangesAsync();

        return membership;
    }

    public async Task<bool> UpdateRoleAsync(Guid id, UserRole role)
    {
        var membership = await _context.OrganizationMemberships
            .FirstOrDefaultAsync(x => x.Id == id);

        if (membership is null)
            return false;

        membership.Role = role;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var membership = await _context.OrganizationMemberships
            .FirstOrDefaultAsync(x => x.Id == id);

        if (membership is null)
            return false;

        _context.OrganizationMemberships.Remove(membership);

        await _context.SaveChangesAsync();

        return true;
    }
}