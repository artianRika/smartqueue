namespace ProfileApi.Models;

public class UserProfile
{
    // Matches the Supabase Auth user ID
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Visitor;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}