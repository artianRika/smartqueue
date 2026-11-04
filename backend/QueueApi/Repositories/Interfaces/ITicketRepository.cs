using QueueApi.DTOs;
using QueueApi.Models;

namespace QueueApi.Repositories.Interfaces;

public interface ITicketRepository
{
    Task<List<Ticket>> GetByQueueAsync(Guid queueId, TicketStatus? status, DateOnly? ticketDate);
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<Ticket> CreateAsync(Guid queueId, Guid userId, string ticketPrefix);
    Task<Ticket?> CallNextAsync(Guid counterId, Guid queueId);
    Task<Ticket?> StartServiceAsync(Guid id);
    Task<Ticket?> CompleteAsync(Guid id);
    Task<Ticket?> CancelAsync(Guid id);
    Task<Ticket?> MarkNoShowAsync(Guid id);
    Task<TicketPositionResponse?> GetPositionAsync(Guid id);
}
