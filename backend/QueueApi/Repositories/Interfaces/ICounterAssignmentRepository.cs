using QueueApi.Models;

namespace QueueApi.Repositories.Interfaces;

public interface ICounterAssignmentRepository
{
    Task<List<CounterAssignment>> GetByCounterIdAsync(Guid counterId);
    Task<CounterAssignment?> GetActiveByCounterIdAsync(Guid counterId);
    Task<CounterAssignment?> GetActiveByStaffUserIdAsync(Guid staffUserId);
    Task<CounterAssignment?> GetByIdAsync(Guid id);
    Task<CounterAssignment> CreateAsync(CounterAssignment assignment);
    Task<bool> EndAsync(Guid id);
}
