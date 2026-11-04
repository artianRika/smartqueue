using Microsoft.EntityFrameworkCore;
using QueueApi.Data;
using QueueApi.Infrastructure;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Repositories;

public class CounterRepository : ICounterRepository
{
    private readonly QueueDbContext _context;

    public CounterRepository(QueueDbContext context)
    {
        _context = context;
    }

    public async Task<List<Counter>> GetByQueueIdAsync(Guid queueId)
    {
        return await _context.Counters
            .AsNoTracking()
            .Where(x => x.QueueId == queueId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Counter?> GetByIdAsync(Guid id)
    {
        return await _context.Counters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Counter> CreateAsync(Counter counter)
    {
        var nameTaken = await _context.Counters.AnyAsync(x =>
            x.QueueId == counter.QueueId &&
            x.Name == counter.Name);

        if (nameTaken)
            throw new ConflictException("A counter with this name already exists on the queue.");

        _context.Counters.Add(counter);
        await RepositorySave.SaveChangesAsync(_context);

        return counter;
    }

    public async Task<bool> UpdateAsync(Counter counter)
    {
        var existing = await _context.Counters
            .FirstOrDefaultAsync(x => x.Id == counter.Id);

        if (existing is null)
            return false;

        var nameTaken = await _context.Counters.AnyAsync(x =>
            x.QueueId == existing.QueueId &&
            x.Id != existing.Id &&
            x.Name == counter.Name);

        if (nameTaken)
            throw new ConflictException("A counter with this name already exists on the queue.");

        existing.Name = counter.Name;
        await RepositorySave.SaveChangesAsync(_context);

        return true;
    }

    public async Task<bool> SetOpenAsync(Guid id, bool isOpen)
    {
        var existing = await _context.Counters
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing is null)
            return false;

        existing.IsOpen = isOpen;
        await RepositorySave.SaveChangesAsync(_context);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _context.Counters
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing is null)
            return false;

        _context.Counters.Remove(existing);
        await RepositorySave.SaveChangesAsync(_context);

        return true;
    }
}
