using Microsoft.EntityFrameworkCore;
using QueueApi.Data;
using QueueApi.DTOs;
using QueueApi.Infrastructure;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly QueueDbContext _context;

    public TicketRepository(QueueDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ticket>> GetByQueueAsync(
        Guid queueId,
        TicketStatus? status,
        DateOnly? ticketDate)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Where(x => x.QueueId == queueId);

        if (status is not null)
            query = query.Where(x => x.Status == status);

        if (ticketDate is not null)
            query = query.Where(x => x.TicketDate == ticketDate);

        return await query
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Number)
            .ToListAsync();
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Ticket> CreateAsync(Guid queueId, Guid userId, string ticketPrefix)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;
        var ticketDate = DateOnly.FromDateTime(now);

        var numberValue = await DbCommands.ScalarAsync(
            _context,
            """
            INSERT INTO queue.queue_daily_counters ("QueueId", "TicketDate", "LastNumber")
            VALUES (@queueId, @ticketDate, 1)
            ON CONFLICT ("QueueId", "TicketDate")
            DO UPDATE SET "LastNumber" = queue.queue_daily_counters."LastNumber" + 1
            RETURNING "LastNumber"
            """,
            ("queueId", queueId),
            ("ticketDate", ticketDate));

        if (numberValue is not int number)
            throw new InvalidOperationException("Failed to issue a ticket number.");

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QueueId = queueId,
            UserId = userId,
            Number = number,
            TicketCode = $"{ticketPrefix}{number}",
            TicketDate = ticketDate,
            Status = TicketStatus.Waiting,
            CreatedAt = now
        };

        _context.Tickets.Add(ticket);
        await RepositorySave.SaveChangesAsync(_context);
        await transaction.CommitAsync();

        return ticket;
    }

    public async Task<Ticket?> CallNextAsync(Guid counterId, Guid queueId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var lockedCounter = await DbCommands.ScalarAsync(
            _context,
            """
            SELECT "Id"
            FROM queue.counters
            WHERE "Id" = @counterId
            FOR UPDATE
            """,
            ("counterId", counterId));

        if (lockedCounter is not Guid)
            return null;

        var hasActiveTicket = await _context.Tickets.AnyAsync(x =>
            x.CounterId == counterId &&
            (x.Status == TicketStatus.Called || x.Status == TicketStatus.Serving));

        if (hasActiveTicket)
            throw new ConflictException("This counter already has a ticket in progress.");

        var calledAt = DateTime.UtcNow;
        var ticketDate = DateOnly.FromDateTime(calledAt);

        var ticketIdValue = await DbCommands.ScalarAsync(
            _context,
            """
            UPDATE queue.tickets
            SET "Status" = 'Called',
                "CounterId" = @counterId,
                "CalledAt" = @calledAt
            WHERE "Id" = (
                SELECT "Id"
                FROM queue.tickets
                WHERE "QueueId" = @queueId
                  AND "TicketDate" = @ticketDate
                  AND "Status" = 'Waiting'
                ORDER BY "CreatedAt", "Number"
                FOR UPDATE SKIP LOCKED
                LIMIT 1
            )
            RETURNING "Id"
            """,
            ("counterId", counterId),
            ("calledAt", calledAt),
            ("queueId", queueId),
            ("ticketDate", ticketDate));

        await transaction.CommitAsync();

        if (ticketIdValue is not Guid ticketId)
            return null;

        return await GetByIdAsync(ticketId);
    }

    public Task<Ticket?> StartServiceAsync(Guid id)
    {
        return TransitionAsync(
            id,
            TicketStatus.Called,
            TicketStatus.Serving,
            ticket => ticket.ServiceStartedAt = DateTime.UtcNow,
            "Only a called ticket can be started.");
    }

    public Task<Ticket?> CompleteAsync(Guid id)
    {
        return TransitionAsync(
            id,
            TicketStatus.Serving,
            TicketStatus.Completed,
            ticket => ticket.CompletedAt = DateTime.UtcNow,
            "Only a ticket in service can be completed.");
    }

    public async Task<Ticket?> CancelAsync(Guid id)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            return null;

        if (ticket.Status is not (TicketStatus.Waiting or TicketStatus.Called))
            throw new ConflictException("Only a waiting or called ticket can be cancelled.");

        ticket.Status = TicketStatus.Cancelled;
        await RepositorySave.SaveChangesAsync(_context);

        return ticket;
    }

    public Task<Ticket?> MarkNoShowAsync(Guid id)
    {
        return TransitionAsync(
            id,
            TicketStatus.Called,
            TicketStatus.NoShow,
            _ => { },
            "Only a called ticket can be marked as a no-show.");
    }

    public async Task<TicketPositionResponse?> GetPositionAsync(Guid id)
    {
        var ticket = await GetByIdAsync(id);

        if (ticket is null)
            return null;

        if (ticket.Status != TicketStatus.Waiting)
        {
            return new TicketPositionResponse
            {
                TicketId = ticket.Id,
                Status = ticket.Status,
                Position = null
            };
        }

        var ahead = await _context.Tickets.CountAsync(x =>
            x.QueueId == ticket.QueueId &&
            x.TicketDate == ticket.TicketDate &&
            x.Status == TicketStatus.Waiting &&
            (x.CreatedAt < ticket.CreatedAt ||
             (x.CreatedAt == ticket.CreatedAt && x.Number < ticket.Number)));

        return new TicketPositionResponse
        {
            TicketId = ticket.Id,
            Status = ticket.Status,
            Position = ahead + 1
        };
    }

    private async Task<Ticket?> TransitionAsync(
        Guid id,
        TicketStatus requiredStatus,
        TicketStatus nextStatus,
        Action<Ticket> apply,
        string conflictMessage)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            return null;

        if (ticket.Status != requiredStatus)
            throw new ConflictException(conflictMessage);

        ticket.Status = nextStatus;
        apply(ticket);
        await RepositorySave.SaveChangesAsync(_context);

        return ticket;
    }
}
