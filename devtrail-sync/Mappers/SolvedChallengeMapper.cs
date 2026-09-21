using devtrail_sync.Models.GitHub;
using devtrail_sync.Models.TableEntities;

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
