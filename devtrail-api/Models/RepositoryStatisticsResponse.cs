namespace devtrail_api.Models;

public class RepositoryStatisticsResponse
{
    public required string Name { get; set; }
    public required string Owner { get; set; }
    public string? Description { get; set; }
    public required List<string> Languages { get; set; }
    public DateTimeOffset? LastCommitDate { get; set; }
    public List<SolvedChallengesResponse>? SolvedChallenges { get; set; }
}
