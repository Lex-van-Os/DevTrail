using devtrail_sync.Models;
using devtrail_sync.Services;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace devtrail_sync;

public class DevTrailSync
{
    private static readonly List<(string Owner, string Name)> TrackedRepositories =
    [
        ("Lex-van-Os", "Steward"),
        ("Lex-van-Os", "code-challenges")
    ];

    private readonly IGitHubSyncService _gitHubSyncService;
    private readonly ILogger<DevTrailSync> _logger;

    public DevTrailSync(IGitHubSyncService gitHubSyncService, ILogger<DevTrailSync> logger)
    {
        _gitHubSyncService = gitHubSyncService;
        _logger = logger;
    }

    [Function(nameof(DevTrailSync))]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer, FunctionContext context)
    {
        _logger.LogInformation("Timer executed");

        foreach ((string owner, string name) in TrackedRepositories)
        {
            try
            {
                RepositorySnapshot repositorySnapshot =
                    await _gitHubSyncService.GetRepositorySnapshotAsync(owner, name);

                _logger.LogInformation("{Repo} description: {Description}", repositorySnapshot.Name,
                    repositorySnapshot.Description);
                _logger.LogInformation("{Repo} languages: {Languages}", repositorySnapshot.Name,
                    string.Join(", ", repositorySnapshot.Languages));
                _logger.LogInformation("{Repo} last commit: {LastCommitDate}", repositorySnapshot.Name,
                    repositorySnapshot.LastCommitDate);

                if (name == "code-challenges")
                {
                    List<SolvedChallengesSnapshot> solvedChallengesSnapshots =
                        await _gitHubSyncService.GetSolvedChallengesSnapshotAsync(owner, name);

                    _logger.LogInformation("{Repo} solved challenges:", repositorySnapshot.Name);

                    foreach (SolvedChallengesSnapshot snapshot in solvedChallengesSnapshots)
                    {
                        _logger.LogInformation("{Repo} language: {Language}", repositorySnapshot.Name,
                            snapshot.Language);

                        _logger.LogInformation("{Language} solved challenges: {SolvedChallenges}",
                            snapshot.Language,
                            snapshot.SolvedChallenges);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to sync {Owner}/{Name}", owner, name);
            }
        }
    }
}