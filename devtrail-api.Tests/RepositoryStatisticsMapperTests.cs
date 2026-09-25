using System.Text.Json;

using devtrail_api.Mappers;
using devtrail_api.Models;

using devtrail_core.Models.TableEntities;

namespace devtrail_api.Tests;

public class RepositoryStatisticsMapperTests
{
    private readonly RepositoryStatisticsMapper _mapper = new();

    [Fact]
    public void Map_MapsRepositoryFieldsAndLanguages()
    {
        RepositoryEntity repositoryEntity = new()
        {
            PartitionKey = "Lex-van-Os",
            RowKey = "Steward",
            Description = "A portfolio dashboard.",
            LanguagesJson = JsonSerializer.Serialize(new List<string> { "TypeScript", "C#" }),
            LastCommitDate = new DateTimeOffset(2026, 8, 22, 8, 46, 1, TimeSpan.Zero)
        };

        List<RepositoryStatisticsResponse> response = _mapper.Map([repositoryEntity], []);

        RepositoryStatisticsResponse result = Assert.Single(response);
        Assert.Equal("Steward", result.Name);
        Assert.Equal("Lex-van-Os", result.Owner);
        Assert.Equal("A portfolio dashboard.", result.Description);
        Assert.Equal(["TypeScript", "C#"], result.Languages);
        Assert.Equal(repositoryEntity.LastCommitDate, result.LastCommitDate);
    }

    [Fact]
    public void Map_JoinsSolvedChallengesByRepositoryName()
    {
        RepositoryEntity repositoryEntity = new()
        {
            PartitionKey = "Lex-van-Os",
            RowKey = "code-challenges",
            LanguagesJson = JsonSerializer.Serialize(new List<string> { "TypeScript" })
        };

        List<SolvedChallengeEntity> solvedChallengeEntities =
        [
            new SolvedChallengeEntity { PartitionKey = "code-challenges", RowKey = "TypeScript", SolvedChallenges = 3 },
            new SolvedChallengeEntity { PartitionKey = "code-challenges", RowKey = "Python", SolvedChallenges = 0 },
            new SolvedChallengeEntity { PartitionKey = "Steward", RowKey = "C#", SolvedChallenges = 5 }
        ];

        List<RepositoryStatisticsResponse> response = _mapper.Map([repositoryEntity], solvedChallengeEntities);

        RepositoryStatisticsResponse result = Assert.Single(response);
        Assert.NotNull(result.SolvedChallenges);
        Assert.Equal(2, result.SolvedChallenges!.Count);
        Assert.Contains(result.SolvedChallenges, solvedChallenge =>
            solvedChallenge.Language == "TypeScript" && solvedChallenge.SolvedChallenges == 3);
        Assert.Contains(result.SolvedChallenges, solvedChallenge =>
            solvedChallenge.Language == "Python" && solvedChallenge.SolvedChallenges == 0);
    }

    [Fact]
    public void Map_ReturnsNullSolvedChallenges_WhenNoMatchExists()
    {
        RepositoryEntity repositoryEntity = new()
        {
            PartitionKey = "Lex-van-Os",
            RowKey = "Steward",
            LanguagesJson = JsonSerializer.Serialize(new List<string>())
        };

        List<RepositoryStatisticsResponse> response = _mapper.Map([repositoryEntity], []);

        Assert.Null(Assert.Single(response).SolvedChallenges);
    }

    [Fact]
    public void Map_MapsMultipleRepositoriesIndependently()
    {
        RepositoryEntity steward = new()
        {
            PartitionKey = "Lex-van-Os",
            RowKey = "Steward",
            LanguagesJson = JsonSerializer.Serialize(new List<string>())
        };
        RepositoryEntity codeChallenges = new()
        {
            PartitionKey = "Lex-van-Os",
            RowKey = "code-challenges",
            LanguagesJson = JsonSerializer.Serialize(new List<string> { "TypeScript" })
        };
        List<SolvedChallengeEntity> solvedChallengeEntities =
        [
            new SolvedChallengeEntity { PartitionKey = "code-challenges", RowKey = "TypeScript", SolvedChallenges = 3 }
        ];

        List<RepositoryStatisticsResponse> response =
            _mapper.Map([steward, codeChallenges], solvedChallengeEntities);

        Assert.Equal(2, response.Count);
        Assert.Null(response.Single(repository => repository.Name == "Steward").SolvedChallenges);
        Assert.NotNull(response.Single(repository => repository.Name == "code-challenges").SolvedChallenges);
    }
}
