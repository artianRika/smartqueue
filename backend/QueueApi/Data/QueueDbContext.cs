using Microsoft.EntityFrameworkCore;
using QueueApi.Models;

namespace QueueApi.Data;

public class QueueDbContext : DbContext
{
    public QueueDbContext(DbContextOptions<QueueDbContext> options)
        : base(options)
    {
    }

    public DbSet<Queue> Queues => Set<Queue>();
    public DbSet<Counter> Counters => Set<Counter>();
    public DbSet<CounterAssignment> CounterAssignments => Set<CounterAssignment>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    
    public DbSet<QueueDailyCounter> QueueDailyCounters
        => Set<QueueDailyCounter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("queue");

        // QUEUES
        modelBuilder.Entity<Queue>(entity =>
        {
            entity.ToTable("queues");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.OrganizationId);

            entity.HasIndex(x => new { x.OrganizationId, x.Name })
                .IsUnique();
            
            entity.HasIndex(x => new { x.OrganizationId, x.TicketPrefix })
                .IsUnique();
            
            entity.Property(x => x.TicketPrefix)
                .IsRequired()
                .HasMaxLength(10);

            entity.ToTable(t =>
                t.HasCheckConstraint(
                    "CK_queues_ticket_prefix",
                    "\"TicketPrefix\" ~ '^[A-Z]{1,10}$'"
                )
            );
        });

        // COUNTERS
        modelBuilder.Entity<Counter>(entity =>
        {
            entity.ToTable("counters");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => new { x.QueueId, x.Name })
                .IsUnique();

            entity.HasOne(x => x.Queue)
                .WithMany(x => x.Counters)
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasAlternateKey(x => new { x.Id, x.QueueId });
        });

        // COUNTER ASSIGNMENTS
        modelBuilder.Entity<CounterAssignment>(entity =>
        {
            entity.ToTable("counter_assignments");

            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Counter)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.CounterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Only one ACTIVE staff assignment per counter
            entity.HasIndex(x => x.CounterId)
                .IsUnique()
                .HasFilter("\"EndedAt\" IS NULL");

            entity.HasIndex(x => x.StaffUserId)
                .IsUnique()
                .HasFilter("\"EndedAt\" IS NULL");
        });

        // TICKETS
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("tickets");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.QueueId,
                x.TicketDate,
                x.Number
            }).IsUnique();

            entity.Property(x => x.TicketCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.ToTable(t =>
                t.HasCheckConstraint(
                    "CK_tickets_number",
                    "\"Number\" > 0"
                )
            );

            entity.HasIndex(x => new { x.QueueId, x.Status, x.CreatedAt });

            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.Queue)
                .WithMany(x => x.Tickets)
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(x => x.Counter)
                .WithMany(x => x.Tickets)
                .HasForeignKey(x => new { x.CounterId, x.QueueId })
                .HasPrincipalKey(x => new { x.Id, x.QueueId })
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<QueueDailyCounter>(entity =>
        {
            entity.ToTable("queue_daily_counters");

            entity.HasKey(x => new { x.QueueId, x.TicketDate });

            entity.HasOne(x => x.Queue)
                .WithMany()
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
                t.HasCheckConstraint(
                    "CK_queue_daily_counters_last_number",
                    "\"LastNumber\" >= 0"
                )
            );
        });
    }
}