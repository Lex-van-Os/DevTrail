using devtrail_core.Models.TableEntities;

using devtrail_sync.Models.GitHub;

namespace devtrail_sync.Mappers;

public interface ISolvedChallengeMapper
{
    SolvedChallengeEntity Map(SolvedChallengesSnapshot solvedChallengesSnapshot, string repositoryName);
}