using Microsoft.EntityFrameworkCore;

namespace NotificationWorker.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("queue");

        modelBuilder.Entity<ProcessedEvent>(entity =>
        {
            entity.ToTable("processed_events");
            entity.HasKey(x => x.EventId);
        });

        modelBuilder.Entity<NotificationRecord>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");
            entity.HasIndex(x => x.EventId).IsUnique();
        });
    }
}
