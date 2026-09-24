using devtrail_core.Models.TableEntities;

namespace devtrail_core.Services;

public interface ITableStorageService
{
    Task UpsertRepositoryData(RepositoryEntity repositoryEntity);
    Task UpsertSolvedChallengeData(SolvedChallengeEntity solvedChallengeEntity);
}