namespace ProfileApi.Models;

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? ClerkUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Visitor;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}