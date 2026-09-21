using System.ComponentModel.DataAnnotations;

namespace devtrail_sync.Models.GitHub;

public class SolvedChallengesSnapshot
{
    [Required] public required string Language { get; set; }
    [Required] public required int SolvedChallenges { get; set; }
}