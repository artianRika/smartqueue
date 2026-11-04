using Microsoft.EntityFrameworkCore;
using QueueApi.Data;
using QueueApi.Infrastructure;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Repositories;

public class CounterAssignmentRepository : ICounterAssignmentRepository
{
    private readonly QueueDbContext _context;

    public CounterAssignmentRepository(QueueDbContext context)
    {
        _context = context;
    }

    public async Task<List<CounterAssignment>> GetByCounterIdAsync(Guid counterId)
    {
        return await _context.CounterAssignments
            .AsNoTracking()
            .Where(x => x.CounterId == counterId)
            .OrderByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<CounterAssignment?> GetActiveByCounterIdAsync(Guid counterId)
    {
        return await _context.CounterAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CounterId == counterId && x.EndedAt == null);
    }

    public async Task<CounterAssignment?> GetActiveByStaffUserIdAsync(Guid staffUserId)
    {
        return await _context.CounterAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StaffUserId == staffUserId && x.EndedAt == null);
    }

    public async Task<CounterAssignment?> GetByIdAsync(Guid id)
    {
        return await _context.CounterAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CounterAssignment> CreateAsync(CounterAssignment assignment)
    {
        var counterBusy = await _context.CounterAssignments.AnyAsync(x =>
            x.CounterId == assignment.CounterId &&
            x.EndedAt == null);

        if (counterBusy)
            throw new ConflictException("This counter already has an active staff assignment.");

        var staffBusy = await _context.CounterAssignments.AnyAsync(x =>
            x.StaffUserId == assignment.StaffUserId &&
            x.EndedAt == null);

        if (staffBusy)
            throw new ConflictException("This staff member is already assigned to a counter.");

        _context.CounterAssignments.Add(assignment);
        await RepositorySave.SaveChangesAsync(_context);

        return assignment;
    }

    public async Task<bool> EndAsync(Guid id)
    {
        var existing = await _context.CounterAssignments
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing is null)
            return false;

        if (existing.EndedAt is not null)
            throw new ConflictException("This assignment has already ended.");

        existing.EndedAt = DateTime.UtcNow;
        await RepositorySave.SaveChangesAsync(_context);

        return true;
    }
}
