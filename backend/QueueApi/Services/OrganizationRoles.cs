namespace QueueApi.Services;

public static class OrganizationRoles
{
    public const string Visitor = "Visitor";
    public const string Staff = "Staff";
    public const string OrganizationAdmin = "OrganizationAdmin";
    public const string SuperAdmin = "SuperAdmin";

    public static bool CanStaffQueue(string? role) =>
        role is Staff or OrganizationAdmin or SuperAdmin;
}
