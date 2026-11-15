namespace QueueApi.Services;

public class ProfileUser
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
