using QueueApi.Models;

namespace QueueApi.DTOs;

public class CounterAssignmentResponse
{
    public Guid Id { get; set; }

    public Guid CounterId { get; set; }

    public Guid StaffUserId { get; set; }

    public string? StaffName { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public static CounterAssignmentResponse From(CounterAssignment assignment, string? staffName)
    {
        return new CounterAssignmentResponse
        {
            Id = assignment.Id,
            CounterId = assignment.CounterId,
            StaffUserId = assignment.StaffUserId,
            StaffName = staffName,
            AssignedAt = assignment.AssignedAt,
            EndedAt = assignment.EndedAt
        };
    }
}
