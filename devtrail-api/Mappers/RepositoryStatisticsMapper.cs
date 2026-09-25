using System.Text.Json;

using devtrail_api.Models;

using devtrail_core.Models.TableEntities;

namespace devtrail_api.Mappers;

public class RepositoryStatisticsMapper : IRepositoryStatisticsMapper
{
    public List<RepositoryStatisticsResponse> Map(List<RepositoryEntity> repositoryEntities,
        List<SolvedChallengeEntity> solvedChallengeEntities)
    {
        return repositoryEntities.Select(repositoryEntity => new RepositoryStatisticsResponse
        {
            Name = repositoryEntity.RowKey,
            Owner = repositoryEntity.PartitionKey,
            Description = repositoryEntity.Description,
            Languages = JsonSerializer.Deserialize<List<string>>(repositoryEntity.LanguagesJson) ?? [],
            LastCommitDate = repositoryEntity.LastCommitDate,
            SolvedChallenges = MapSolvedChallenges(repositoryEntity.RowKey, solvedChallengeEntities)
        }).ToList();
    }

    private static List<SolvedChallengesResponse>? MapSolvedChallenges(string repositoryName,
        List<SolvedChallengeEntity> solvedChallengeEntities)
    {
        List<SolvedChallengesResponse> matches = solvedChallengeEntities
            .Where(solvedChallengeEntity => solvedChallengeEntity.PartitionKey == repositoryName)
            .Select(solvedChallengeEntity => new SolvedChallengesResponse
            {
                Language = solvedChallengeEntity.RowKey,
                SolvedChallenges = solvedChallengeEntity.SolvedChallenges
            })
            .ToList();

        return matches.Count > 0 ? matches : null;
    }
}
