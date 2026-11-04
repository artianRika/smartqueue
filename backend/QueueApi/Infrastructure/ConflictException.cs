namespace QueueApi.Infrastructure;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
