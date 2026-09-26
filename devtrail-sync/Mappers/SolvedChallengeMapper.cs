using devtrail_core.Models.TableEntities;

using devtrail_sync.Models.GitHub;

namespace devtrail_sync.Mappers;

public class SolvedChallengeMapper : ISolvedChallengeMapper
{
    public SolvedChallengeEntity Map(SolvedChallengesSnapshot solvedChallengesSnapshot, string repositoryName)
    {
        return new SolvedChallengeEntity
        {
            PartitionKey = repositoryName,
            RowKey = solvedChallengesSnapshot.Language,
            SolvedChallenges = solvedChallengesSnapshot.SolvedChallenges
        };
    }
}