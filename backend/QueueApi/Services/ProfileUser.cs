namespace QueueApi.Services;

public class ProfileUser
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
