using Microsoft.EntityFrameworkCore;
using QueueApi.Models;

namespace QueueApi.Data;

public static class QueueDbSeeder
{
    public static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AlexVisitorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SamStaffId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DanaStaffId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid MorganAdminId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid JamieVisitorId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static readonly Guid RegistrarQueueId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid FinancialAidQueueId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid ItHelpQueueId = Guid.Parse("10000000-0000-0000-0000-000000000003");

    public static readonly Guid RegistrarCounter1Id = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid RegistrarCounter2Id = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid FinancialAidCounterId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid ItHelpCounterId = Guid.Parse("20000000-0000-0000-0000-000000000004");

    public static readonly Guid FirstWaitingTicketId = Guid.Parse("40000000-0000-0000-0000-000000000005");

    public static async Task SeedAsync(QueueDbContext context)
    {
        await context.Database.MigrateAsync();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        await ResetOrganizationQueuesAsync(context);

        context.Queues.AddRange(
            NewQueue(RegistrarQueueId, "Student Registrar", "Enrollment and documents", "REG", true, now),
            NewQueue(FinancialAidQueueId, "Financial Aid", "Tuition and funding", "FIN", true, now),
            NewQueue(ItHelpQueueId, "IT Help Desk", "Accounts and devices", "IT", false, now));

        context.Counters.AddRange(
            NewCounter(RegistrarCounter1Id, RegistrarQueueId, "Counter 1", true, now),
            NewCounter(RegistrarCounter2Id, RegistrarQueueId, "Counter 2", true, now),
            NewCounter(FinancialAidCounterId, FinancialAidQueueId, "Counter 1", true, now),
            NewCounter(ItHelpCounterId, ItHelpQueueId, "Desk", false, now));

        await context.SaveChangesAsync();

        context.CounterAssignments.AddRange(
            new CounterAssignment
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                CounterId = RegistrarCounter1Id,
                StaffUserId = MorganAdminId,
                AssignedAt = now.AddHours(-5),
                EndedAt = now.AddHours(-2)
            },
            new CounterAssignment
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                CounterId = RegistrarCounter1Id,
                StaffUserId = SamStaffId,
                AssignedAt = now.AddHours(-2)
            },
            new CounterAssignment
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                CounterId = RegistrarCounter2Id,
                StaffUserId = DanaStaffId,
                AssignedAt = now.AddHours(-2)
            });

        AddTicket(context, "40000000-0000-0000-0000-000000000001", RegistrarQueueId, AlexVisitorId, RegistrarCounter1Id, 1, "REG", today, TicketStatus.Completed, now.AddMinutes(-50), now.AddMinutes(-48), now.AddMinutes(-45), now.AddMinutes(-30));
        AddTicket(context, "40000000-0000-0000-0000-000000000002", RegistrarQueueId, JamieVisitorId, RegistrarCounter1Id, 2, "REG", today, TicketStatus.NoShow, now.AddMinutes(-40), now.AddMinutes(-38), null, null);
        AddTicket(context, "40000000-0000-0000-0000-000000000003", RegistrarQueueId, AlexVisitorId, null, 3, "REG", today, TicketStatus.Cancelled, now.AddMinutes(-30), null, null, null);
        AddTicket(context, "40000000-0000-0000-0000-000000000004", RegistrarQueueId, JamieVisitorId, RegistrarCounter2Id, 4, "REG", today, TicketStatus.Serving, now.AddMinutes(-20), now.AddMinutes(-18), now.AddMinutes(-15), null);
        AddTicket(context, "40000000-0000-0000-0000-000000000005", RegistrarQueueId, AlexVisitorId, null, 5, "REG", today, TicketStatus.Waiting, now.AddMinutes(-10), null, null, null);
        AddTicket(context, "40000000-0000-0000-0000-000000000006", RegistrarQueueId, JamieVisitorId, null, 6, "REG", today, TicketStatus.Waiting, now.AddMinutes(-5), null, null, null);
        AddTicket(context, "40000000-0000-0000-0000-000000000007", RegistrarQueueId, AlexVisitorId, null, 7, "REG", today, TicketStatus.Waiting, now.AddMinutes(-1), null, null, null);
        AddTicket(context, "40000000-0000-0000-0000-000000000008", FinancialAidQueueId, JamieVisitorId, null, 1, "FIN", today, TicketStatus.Waiting, now.AddMinutes(-8), null, null, null);

        context.QueueDailyCounters.AddRange(
            new QueueDailyCounter { QueueId = RegistrarQueueId, TicketDate = today, LastNumber = 7 },
            new QueueDailyCounter { QueueId = FinancialAidQueueId, TicketDate = today, LastNumber = 1 });

        await context.SaveChangesAsync();

        Console.WriteLine("Queue seed ready.");
        Console.WriteLine($"Registrar queue {RegistrarQueueId} is open. Next ticket is {FirstWaitingTicketId} (REG5).");
        Console.WriteLine($"Counter 1 {RegistrarCounter1Id} is staffed by Sam and can call next.");
        Console.WriteLine($"Counter 2 {RegistrarCounter2Id} is staffed by Dana and is already serving REG4.");
        Console.WriteLine($"Financial Aid queue {FinancialAidQueueId} is open with one waiting ticket.");
        Console.WriteLine($"IT Help Desk queue {ItHelpQueueId} is closed.");
    }

    private static async Task ResetOrganizationQueuesAsync(QueueDbContext context)
    {
        var queueIds = await context.Queues
            .Where(x => x.OrganizationId == OrganizationId)
            .Select(x => x.Id)
            .ToListAsync();

        if (queueIds.Count == 0)
            return;

        var counterIds = await context.Counters
            .Where(x => queueIds.Contains(x.QueueId))
            .Select(x => x.Id)
            .ToListAsync();

        context.Tickets.RemoveRange(
            await context.Tickets.Where(x => queueIds.Contains(x.QueueId)).ToListAsync());
        context.CounterAssignments.RemoveRange(
            await context.CounterAssignments.Where(x => counterIds.Contains(x.CounterId)).ToListAsync());
        context.QueueDailyCounters.RemoveRange(
            await context.QueueDailyCounters.Where(x => queueIds.Contains(x.QueueId)).ToListAsync());
        await context.SaveChangesAsync();

        context.Counters.RemoveRange(
            await context.Counters.Where(x => queueIds.Contains(x.QueueId)).ToListAsync());
        await context.SaveChangesAsync();

        context.Queues.RemoveRange(
            await context.Queues.Where(x => queueIds.Contains(x.Id)).ToListAsync());
        await context.SaveChangesAsync();
    }

    private static Queue NewQueue(
        Guid id,
        string name,
        string description,
        string ticketPrefix,
        bool isOpen,
        DateTime createdAt)
    {
        return new Queue
        {
            Id = id,
            OrganizationId = OrganizationId,
            Name = name,
            Description = description,
            TicketPrefix = ticketPrefix,
            IsOpen = isOpen,
            CreatedAt = createdAt
        };
    }

    private static Counter NewCounter(Guid id, Guid queueId, string name, bool isOpen, DateTime createdAt)
    {
        return new Counter
        {
            Id = id,
            QueueId = queueId,
            Name = name,
            IsOpen = isOpen,
            CreatedAt = createdAt
        };
    }

    private static void AddTicket(
        QueueDbContext context,
        string id,
        Guid queueId,
        Guid userId,
        Guid? counterId,
        int number,
        string prefix,
        DateOnly ticketDate,
        TicketStatus status,
        DateTime createdAt,
        DateTime? calledAt,
        DateTime? serviceStartedAt,
        DateTime? completedAt)
    {
        context.Tickets.Add(new Ticket
        {
            Id = Guid.Parse(id),
            QueueId = queueId,
            UserId = userId,
            CounterId = counterId,
            Number = number,
            TicketCode = $"{prefix}{number}",
            TicketDate = ticketDate,
            Status = status,
            CreatedAt = createdAt,
            CalledAt = calledAt,
            ServiceStartedAt = serviceStartedAt,
            CompletedAt = completedAt
        });
    }
}
