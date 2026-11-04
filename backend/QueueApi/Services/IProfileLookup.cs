namespace QueueApi.Services;

public interface IProfileLookup
{
    Task<string?> GetOrganizationNameAsync(Guid organizationId);

    Task<IReadOnlyDictionary<Guid, string>> GetOrganizationNamesAsync(IEnumerable<Guid> organizationIds);

    Task<ProfileUser?> GetUserAsync(Guid userId);

    Task<IReadOnlyDictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds);
}
