using Microsoft.EntityFrameworkCore;
using ProfileApi.Models;

namespace ProfileApi.Data;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<OrganizationMembership> OrganizationMemberships
        => Set<OrganizationMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // All tables belong to the "profile" schema
        modelBuilder.HasDefaultSchema("profile");

        // ORGANIZATIONS TABLE
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

        // USER_PROFILES TABLE
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");

            entity.HasKey(x => x.Id);
            
            
            
            entity.Property(x => x.ClerkUserId)
                
                .HasMaxLength(255);

            entity.HasIndex(x => x.ClerkUserId)
                .IsUnique();

            entity.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            // Store enum as text: Visitor, Staff, Organization Admin, System Admin
            entity.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });
        
        //Org Membership Table
        modelBuilder.Entity<OrganizationMembership>(entity =>
        {
            entity.ToTable("organization_memberships");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.UserId, x.OrganizationId })
                .IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });
    }
}