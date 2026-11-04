using QueueApi.Models;

namespace QueueApi.Repositories.Interfaces;

public interface ICounterRepository
{
    Task<List<Counter>> GetByQueueIdAsync(Guid queueId);
    Task<Counter?> GetByIdAsync(Guid id);
    Task<Counter> CreateAsync(Counter counter);
    Task<bool> UpdateAsync(Counter counter);
    Task<bool> SetOpenAsync(Guid id, bool isOpen);
    Task<bool> DeleteAsync(Guid id);
}
