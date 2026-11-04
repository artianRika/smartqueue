using Microsoft.EntityFrameworkCore;
using ProfileApi.Models;

namespace ProfileApi.Data;

public static class ProfileDbSeeder
{
    public static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AlexVisitorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SamStaffId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DanaStaffId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid MorganAdminId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid JamieVisitorId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static async Task SeedAsync(ProfileDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Organizations.AnyAsync(x => x.Id == OrganizationId))
        {
            context.Organizations.Add(new Organization
            {
                Id = OrganizationId,
                Name = "Heilbronn University",
                CreatedAt = DateTime.UtcNow
            });
        }

        await AddUserIfMissingAsync(context, AlexVisitorId, "Alex Visitor", "alex.visitor@example.com", UserRole.Visitor);
        await AddUserIfMissingAsync(context, JamieVisitorId, "Jamie Visitor", "jamie.visitor@example.com", UserRole.Visitor);
        await AddUserIfMissingAsync(context, SamStaffId, "Sam Staff", "sam.staff@example.com", UserRole.Staff);
        await AddUserIfMissingAsync(context, DanaStaffId, "Dana Staff", "dana.staff@example.com", UserRole.Staff);
        await AddUserIfMissingAsync(context, MorganAdminId, "Morgan Admin", "morgan.admin@example.com", UserRole.Admin);

        await context.SaveChangesAsync();

        Console.WriteLine("Profile seed ready.");
        Console.WriteLine($"Organization {OrganizationId} Heilbronn University");
        Console.WriteLine($"Visitor {AlexVisitorId} Alex Visitor");
        Console.WriteLine($"Visitor {JamieVisitorId} Jamie Visitor");
        Console.WriteLine($"Staff   {SamStaffId} Sam Staff");
        Console.WriteLine($"Staff   {DanaStaffId} Dana Staff");
        Console.WriteLine($"Admin   {MorganAdminId} Morgan Admin");
    }

    private static async Task AddUserIfMissingAsync(
        ProfileDbContext context,
        Guid id,
        string fullName,
        string email,
        UserRole role)
    {
        if (await context.UserProfiles.AnyAsync(x => x.Id == id))
            return;

        context.UserProfiles.Add(new UserProfile
        {
            Id = id,
            FullName = fullName,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow
        });
    }
}
