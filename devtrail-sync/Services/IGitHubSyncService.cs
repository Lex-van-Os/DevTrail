using devtrail_sync.Models;

namespace devtrail_sync.Services;

public interface IGitHubSyncService
{
    Task<RepositorySnapshot> GetRepositorySnapshotAsync(string owner, string repositoryName);
}