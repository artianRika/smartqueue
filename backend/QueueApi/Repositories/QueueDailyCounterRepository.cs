using Microsoft.EntityFrameworkCore;
using QueueApi.Data;
using QueueApi.Models;
using QueueApi.Repositories.Interfaces;

namespace QueueApi.Repositories;

public class QueueDailyCounterRepository : IQueueDailyCounterRepository
{
    private readonly QueueDbContext _context;

    public QueueDailyCounterRepository(QueueDbContext context)
    {
        _context = context;
    }

    public async Task<List<QueueDailyCounter>> GetByQueueIdAsync(Guid queueId)
    {
        return await _context.QueueDailyCounters
            .AsNoTracking()
            .Where(x => x.QueueId == queueId)
            .OrderByDescending(x => x.TicketDate)
            .ToListAsync();
    }

    public async Task<QueueDailyCounter?> GetAsync(Guid queueId, DateOnly ticketDate)
    {
        return await _context.QueueDailyCounters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.QueueId == queueId && x.TicketDate == ticketDate);
    }
}
