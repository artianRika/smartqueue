using QueueApi.DTOs;
using QueueApi.Models;

namespace QueueApi.Repositories.Interfaces;

public interface IQueueRepository
{
    Task<List<Queue>> GetAllAsync(Guid? organizationId);
    Task<Queue?> GetByIdAsync(Guid id);
    Task<Queue> CreateAsync(Queue queue);
    Task<bool> UpdateAsync(Queue queue);
    Task<bool> SetOpenAsync(Guid id, bool isOpen);
    Task<bool> DeleteAsync(Guid id);
    Task<WaitEstimateResponse> GetWaitEstimateAsync(Queue queue);
}
