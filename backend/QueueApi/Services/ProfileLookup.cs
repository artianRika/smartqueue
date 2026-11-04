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

    public async Task<IReadOnlyDictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds)
    {
        var users = await QueryUsersAsync(userIds);

        return users.ToDictionary(pair => pair.Key, pair => pair.Value.FullName);
    }

    private async Task<Dictionary<Guid, string>> QueryNamesAsync(string sql, IEnumerable<Guid> ids)
    {
        var rows = new Dictionary<Guid, string>();

        await QueryAsync(sql, ids, reader =>
        {
            rows[reader.GetGuid(0)] = reader.GetString(1);
        });

        return rows;
    }

    private async Task<Dictionary<Guid, ProfileUser>> QueryUsersAsync(IEnumerable<Guid> ids)
    {
        var users = new Dictionary<Guid, ProfileUser>();

        await QueryAsync(
            """
            SELECT "Id", "FullName", "Role"
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
                    FullName = reader.GetString(1),
                    Role = reader.GetString(2)
                };
            });

        return users;
    }

    private async Task QueryAsync(string sql, IEnumerable<Guid> ids, Action<NpgsqlDataReader> read)
    {
        var idList = ids.Where(id => id != Guid.Empty).Distinct().ToArray();

        if (idList.Length == 0)
            return;

        var connection = (NpgsqlConnection)_context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Transaction = _context.Database.CurrentTransaction?.GetDbTransaction() as NpgsqlTransaction
        };

        command.Parameters.Add(new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid)
        {
            Value = idList
        });

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            read(reader);
    }
}
