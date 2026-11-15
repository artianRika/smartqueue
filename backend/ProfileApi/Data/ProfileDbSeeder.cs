using Microsoft.EntityFrameworkCore;
using ProfileApi.Models;

namespace ProfileApi.Data;

public static class ProfileDbSeeder
{
    public static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AlexVisitorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SamStaffId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DanaStaffId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid MorganOrgAdminId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid JamieVisitorId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid BossSysAdminId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    public static async Task SeedAsync(ProfileDbContext context)
    {
        await context.Database.MigrateAsync();

        // 1. Create organization
        if (!await context.Organizations.AnyAsync(x => x.Id == OrganizationId))
        {
            context.Organizations.Add(new Organization
            {
                Id = OrganizationId,
                Name = "Heilbronn University",
                CreatedAt = DateTime.UtcNow
            });
        }

        // 2. Create users with GLOBAL roles
        await AddUserIfMissingAsync(context, AlexVisitorId, "Alex", "Visitor",
            "alex.visitor@example.com", UserRole.Visitor);

        await AddUserIfMissingAsync(context, JamieVisitorId, "Jamie", "Visitor",
            "jamie.visitor@example.com", UserRole.Visitor);

        await AddUserIfMissingAsync(context, SamStaffId, "Sam", "Staff",
            "sam.staff@example.com", UserRole.Visitor);

        await AddUserIfMissingAsync(context, DanaStaffId, "Dana", "Staff",
            "dana.staff@example.com", UserRole.Visitor);

        await AddUserIfMissingAsync(context, MorganOrgAdminId, "Morgan", "Admin",
            "morgan.admin@example.com", UserRole.Visitor);

        await AddUserIfMissingAsync(context, BossSysAdminId, "Boss", "Admin",
            "boss.admin@example.com", UserRole.SuperAdmin);

        await context.SaveChangesAsync();

        // 3. Assign ORGANIZATION roles
        await AddMembershipIfMissingAsync(
            context, AlexVisitorId, OrganizationId, UserRole.Visitor);

        await AddMembershipIfMissingAsync(
            context, JamieVisitorId, OrganizationId, UserRole.Visitor);

        await AddMembershipIfMissingAsync(
            context, SamStaffId, OrganizationId, UserRole.Staff);

        await AddMembershipIfMissingAsync(
            context, DanaStaffId, OrganizationId, UserRole.Staff);

        await AddMembershipIfMissingAsync(
            context, MorganOrgAdminId, OrganizationId, UserRole.OrganizationAdmin);

        // Boss is SuperAdmin globally and needs no organization membership.

        await context.SaveChangesAsync();

        Console.WriteLine("Profile seed ready.");
        Console.WriteLine($"OrganizationAdmin: Morgan ({MorganOrgAdminId})");
        Console.WriteLine($"Staff: Sam ({SamStaffId})");
        Console.WriteLine($"Staff: Dana ({DanaStaffId})");
        Console.WriteLine($"Visitors: Alex, Jamie");
        Console.WriteLine($"SuperAdmin: Boss ({BossSysAdminId})");
    }

    private static async Task AddUserIfMissingAsync(
        ProfileDbContext context,
        Guid id,
        string firstName,
        string lastName,
        string email,
        UserRole role)
    {
        var user = await context.UserProfiles.FindAsync(id);

        if (user is not null)
        {
            // Synchronize roles for existing development seed users.
            user.Role = role;
            return;
        }

        context.UserProfiles.Add(new UserProfile
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static async Task AddMembershipIfMissingAsync(
        ProfileDbContext context,
        Guid userId,
        Guid organizationId,
        UserRole role)
    {
        var membership = await context.OrganizationMemberships
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.OrganizationId == organizationId);

        if (membership is not null)
        {
            membership.Role = role;
            return;
        }

        context.OrganizationMemberships.Add(new OrganizationMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = organizationId,
            Role = role,
            CreatedAt = DateTime.UtcNow
        });
    }
}