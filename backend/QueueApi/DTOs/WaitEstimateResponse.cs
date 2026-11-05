namespace QueueApi.DTOs;

public class WaitEstimateResponse
{
    public Guid QueueId { get; set; }

    public bool IsOpen { get; set; }

    public int WaitingCount { get; set; }

    // Open counters with a staff member currently assigned
    public int ActiveCounters { get; set; }

    public double AverageServiceMinutes { get; set; }

    // Completed tickets today that the average is based on; 0 means the default was used
    public int ServiceTimeSampleCount { get; set; }

    // Null when the queue is closed or no counter is staffed
    public int? EstimatedWaitMinutes { get; set; }
}
