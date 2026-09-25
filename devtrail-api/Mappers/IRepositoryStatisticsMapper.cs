using devtrail_api.Models;

using devtrail_core.Models.TableEntities;

namespace devtrail_api.Mappers;

public interface IRepositoryStatisticsMapper
{
    List<RepositoryStatisticsResponse> Map(List<RepositoryEntity> repositoryEntities,
        List<SolvedChallengeEntity> solvedChallengeEntities);
}
