using Microsoft.EntityFrameworkCore;
using QueueApi.Data;
using QueueApi.DTOs;
using QueueApi.Infrastructure;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Repositories;

public class QueueRepository : IQueueRepository
{
    private readonly QueueDbContext _context;

    private const double DefaultServiceMinutes = 5;
    private const int ServiceTimeSampleSize = 20;

    public QueueRepository(QueueDbContext context)
    {
        _context = context;
    }

    public async Task<WaitEstimateResponse> GetWaitEstimateAsync(Queue queue)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var waitingCount = await _context.Tickets.CountAsync(x =>
            x.QueueId == queue.Id &&
            x.TicketDate == today &&
            x.Status == TicketStatus.Waiting);

        var activeCounters = await _context.Counters.CountAsync(x =>
            x.QueueId == queue.Id &&
            x.IsOpen &&
            x.Assignments.Any(a => a.EndedAt == null));

        var recentServices = await _context.Tickets
            .AsNoTracking()
            .Where(x =>
                x.QueueId == queue.Id &&
                x.TicketDate == today &&
                x.Status == TicketStatus.Completed &&
                x.ServiceStartedAt != null &&
                x.CompletedAt != null)
            .OrderByDescending(x => x.CompletedAt)
            .Take(ServiceTimeSampleSize)
            .Select(x => new { x.ServiceStartedAt, x.CompletedAt })
            .ToListAsync();

        var averageServiceMinutes = recentServices.Count == 0
            ? DefaultServiceMinutes
            : recentServices.Average(x => (x.CompletedAt!.Value - x.ServiceStartedAt!.Value).TotalMinutes);

        int? estimatedWaitMinutes = queue.IsOpen && activeCounters > 0
            ? (int)Math.Ceiling(Math.Ceiling((double)waitingCount / activeCounters) * averageServiceMinutes)
            : null;

        return new WaitEstimateResponse
        {
            QueueId = queue.Id,
            IsOpen = queue.IsOpen,
            WaitingCount = waitingCount,
            ActiveCounters = activeCounters,
            AverageServiceMinutes = Math.Round(averageServiceMinutes, 1),
            ServiceTimeSampleCount = recentServices.Count,
            EstimatedWaitMinutes = estimatedWaitMinutes
        };
    }

    public async Task<List<Queue>> GetAllAsync(Guid? organizationId)
    {
        var query = _context.Queues.AsNoTracking();

        if (organizationId is not null)
            query = query.Where(x => x.OrganizationId == organizationId);

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Queue?> GetByIdAsync(Guid id)
    {
        return await _context.Queues
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Queue> CreateAsync(Queue queue)
    {
        var nameTaken = await _context.Queues.AnyAsync(x =>
            x.OrganizationId == queue.OrganizationId &&
            x.Name == queue.Name);

        if (nameTaken)
            throw new ConflictException("A queue with this name already exists for the organization.");

        var prefixTaken = await _context.Queues.AnyAsync(x =>
            x.OrganizationId == queue.OrganizationId &&
            x.TicketPrefix == queue.TicketPrefix);

        if (prefixTaken)
            throw new ConflictException("This ticket prefix is already used by the organization.");

        _context.Queues.Add(queue);
        await RepositorySave.SaveChangesAsync(_context);

        return queue;
    }

    public async Task<bool> UpdateAsync(Queue queue)
    {
        var existing = await _context.Queues
            .FirstOrDefaultAsync(x => x.Id == queue.Id);

        if (existing is null)
            return false;

        var nameTaken = await _context.Queues.AnyAsync(x =>
            x.OrganizationId == existing.OrganizationId &&
            x.Id != existing.Id &&
            x.Name == queue.Name);

        if (nameTaken)
            throw new ConflictException("A queue with this name already exists for the organization.");

        existing.Name = queue.Name;
        existing.Description = queue.Description;

        await RepositorySave.SaveChangesAsync(_context);

        return true;
    }

    public async Task<bool> SetOpenAsync(Guid id, bool isOpen)
    {
        var existing = await _context.Queues
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing is null)
            return false;

        existing.IsOpen = isOpen;
        await RepositorySave.SaveChangesAsync(_context);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _context.Queues
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing is null)
            return false;

        _context.Queues.Remove(existing);
        await RepositorySave.SaveChangesAsync(_context);

        return true;
    }
}
