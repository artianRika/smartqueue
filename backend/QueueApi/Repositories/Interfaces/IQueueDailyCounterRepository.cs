using QueueApi.Models;

namespace QueueApi.Repositories.Interfaces;

public interface IQueueDailyCounterRepository
{
    Task<List<QueueDailyCounter>> GetByQueueIdAsync(Guid queueId);
    Task<QueueDailyCounter?> GetAsync(Guid queueId, DateOnly ticketDate);
}
