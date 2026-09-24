using System.Text.Json;

using devtrail_core.Models.TableEntities;

using devtrail_sync.Models.GitHub;

namespace devtrail_sync.Mappers;

public class RepositoryMapper : IRepositoryMapper
{
    public RepositoryEntity Map(RepositorySnapshot repositorySnapshot)
    {
        return new RepositoryEntity
        {
            PartitionKey = repositorySnapshot.Owner,
            RowKey = repositorySnapshot.Name,
            Description = repositorySnapshot.Description,
            LanguagesJson = JsonSerializer.Serialize(repositorySnapshot.Languages),
            LastCommitDate = repositorySnapshot.LastCommitDate
        };
    }
}