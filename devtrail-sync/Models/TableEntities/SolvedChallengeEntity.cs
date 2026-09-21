using Azure;
using Azure.Data.Tables;

namespace devtrail_sync.Models.TableEntities;

public class SolvedChallengeEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public int SolvedChallenges { get; set; }
}
