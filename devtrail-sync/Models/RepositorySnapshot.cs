using System.ComponentModel.DataAnnotations;

namespace devtrail_sync.Models;

public class RepositorySnapshot
{
    public string? Description { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required string Owner { get; set; }
    [Required] public required List<string> Languages { get; set; }
    public DateTimeOffset? LastCommitDate { get; set; }
}