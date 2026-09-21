using devtrail_sync.Models.TableEntities;

namespace devtrail_sync.Services;

public class TableStorageService : ITableStorageService
{
    public async Task<int> UpsertRepositoryData(RepositoryEntity repositoryEntity)
    {
        return 1;
    }

    public async Task<int> UpsertSolvedChallengeData(SolvedChallengeEntity solvedChallengeEntity)
    {
        return 1;
    }
}