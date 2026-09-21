using devtrail_sync.Models.GitHub;

namespace devtrail_sync.Services;

public interface IGitHubSyncService
{
    Task<RepositorySnapshot> GetRepositorySnapshotAsync(string owner, string repositoryName);
    Task<List<SolvedChallengesSnapshot>> GetSolvedChallengesSnapshotAsync(string owner, string repositoryName);
}