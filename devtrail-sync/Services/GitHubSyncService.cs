using devtrail_sync.Models;

using Microsoft.Extensions.Logging;

using Octokit;

namespace devtrail_sync.Services;

public class GitHubSyncService : IGitHubSyncService
{
    private readonly GitHubClient _client;
    private readonly ILogger<GitHubSyncService> _logger;

    public GitHubSyncService(
        ILogger<GitHubSyncService> logger, GitHubClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<RepositorySnapshot> GetRepositorySnapshotAsync(string owner, string repositoryName)
    {
        Repository repository = await _client.Repository.Get(owner, repositoryName);

        IReadOnlyList<RepositoryLanguage> languages = await _client.Repository.GetAllLanguages(owner, repositoryName);

        IReadOnlyList<GitHubCommit> commits =
            await _client.Repository.Commit.GetAll(owner, repositoryName,
                new ApiOptions { PageSize = 1, PageCount = 1 });

        DateTimeOffset? lastCommitDate = commits.FirstOrDefault()?.Commit.Author.Date;

        return new RepositorySnapshot
        {
            Name = repositoryName,
            Owner = owner,
            Description = repository.Description,
            Languages = languages.Select(l => l.Name).ToList(),
            LastCommitDate = lastCommitDate
        };
    }

    public async Task<List<SolvedChallengesSnapshot>> GetSolvedChallengesSnapshotAsync(string owner,
        string repositoryName)
    {
        IReadOnlyList<RepositoryContent> rootContents =
            await _client.Repository.Content.GetAllContents(owner, repositoryName);

        List<SolvedChallengesSnapshot> snapshots = [];

        foreach (RepositoryContent languageFolder in rootContents.Where(content => content.Type == ContentType.Dir))
        {
            IReadOnlyList<RepositoryContent> challengeFolders =
                await _client.Repository.Content.GetAllContents(owner, repositoryName, languageFolder.Path);

            snapshots.Add(new SolvedChallengesSnapshot
            {
                Language = languageFolder.Name,
                SolvedChallenges = challengeFolders.Count(content => content.Type == ContentType.Dir)
            });
        }

        return snapshots;
    }
}