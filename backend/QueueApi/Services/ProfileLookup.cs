using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;
using QueueApi.Data;

namespace QueueApi.Services;

public class ProfileLookup : IProfileLookup
{
    private readonly QueueDbContext _context;

    public ProfileLookup(QueueDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetOrganizationNameAsync(Guid organizationId)
    {
        var names = await GetOrganizationNamesAsync([organizationId]);

        return names.TryGetValue(organizationId, out var name) ? name : null;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetOrganizationNamesAsync(
        IEnumerable<Guid> organizationIds)
    {
        return await QueryNamesAsync(
            """
            SELECT "Id", "Name"
            FROM profile.organizations
            WHERE "Id" = ANY(@ids)
            """,
            organizationIds);
    }

    public async Task<ProfileUser?> GetUserAsync(Guid userId)
    {
        var users = await QueryUsersAsync([userId]);

        return users.TryGetValue(userId, out var user) ? user : null;
    }

    public async Task<ProfileUser?> GetUserByClerkIdAsync(string clerkUserId)
    {
        if (string.IsNullOrWhiteSpace(clerkUserId))
            return null;

        var connection = await OpenConnectionAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT "Id", "FirstName", "LastName", "Role"
            FROM profile.user_profiles
            WHERE "ClerkUserId" = @clerkUserId
            LIMIT 1
            """,
            connection)
        {
            Transaction = CurrentTransaction()
        };

        command.Parameters.AddWithValue("clerkUserId", clerkUserId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new ProfileUser
        {
            Id = reader.GetGuid(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            Role = reader.GetString(3)
        };
    }

    public async Task<string?> GetOrganizationRoleAsync(Guid userId, Guid organizationId)
    {
        var user = await GetUserAsync(userId);

        if (user is null)
            return null;

        if (user.Role == OrganizationRoles.SuperAdmin)
            return OrganizationRoles.SuperAdmin;

        var connection = await OpenConnectionAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT "Role"
            FROM profile.organization_memberships
            WHERE "UserId" = @userId AND "OrganizationId" = @organizationId
            LIMIT 1
            """,
            connection)
        {
            Transaction = CurrentTransaction()
        };

        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("organizationId", organizationId);

        var role = await command.ExecuteScalarAsync();

        return role as string;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetUserNamesAsync(
        IEnumerable<Guid> userIds)
    {
        var users = await QueryUsersAsync(userIds);

        return users.ToDictionary(
            pair => pair.Key,
            pair => $"{pair.Value.FirstName} {pair.Value.LastName}".Trim());
    }

    private async Task<Dictionary<Guid, string>> QueryNamesAsync(
        string sql,
        IEnumerable<Guid> ids)
    {
        var rows = new Dictionary<Guid, string>();

        await QueryAsync(sql, ids, reader =>
        {
            rows[reader.GetGuid(0)] = reader.GetString(1);
        });

        return rows;
    }

    private async Task<Dictionary<Guid, ProfileUser>> QueryUsersAsync(
        IEnumerable<Guid> ids)
    {
        var users = new Dictionary<Guid, ProfileUser>();

        await QueryAsync(
            """
            SELECT "Id", "FirstName", "LastName", "Role"
            FROM profile.user_profiles
            WHERE "Id" = ANY(@ids)
            """,
            ids,
            reader =>
            {
                var id = reader.GetGuid(0);

                users[id] = new ProfileUser
                {
                    Id = id,
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Role = reader.GetString(3)
                };
            });

        return users;
    }

    private async Task QueryAsync(
        string sql,
        IEnumerable<Guid> ids,
        Action<NpgsqlDataReader> read)
    {
        var idList = ids
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (idList.Length == 0)
            return;

        var connection = await OpenConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Transaction = CurrentTransaction()
        };

        command.Parameters.Add(
            new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid)
            {
                Value = idList
            });

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            read(reader);
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connection = (NpgsqlConnection)_context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        return connection;
    }

    private NpgsqlTransaction? CurrentTransaction() =>
        _context.Database.CurrentTransaction?.GetDbTransaction() as NpgsqlTransaction;
}
