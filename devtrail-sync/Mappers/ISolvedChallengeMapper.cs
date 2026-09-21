using devtrail_sync.Models.GitHub;
using devtrail_sync.Models.TableEntities;

namespace devtrail_sync.Mappers;

public interface ISolvedChallengeMapper
{
    SolvedChallengeEntity Map(SolvedChallengesSnapshot solvedChallengesSnapshot, string repositoryName);
}
