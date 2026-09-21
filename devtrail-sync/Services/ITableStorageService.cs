using devtrail_sync.Models.TableEntities;

namespace devtrail_sync.Services;

public interface ITableStorageService
{
    Task UpsertRepositoryData(RepositoryEntity repositoryEntity);
    Task UpsertSolvedChallengeData(SolvedChallengeEntity solvedChallengeEntity);
}