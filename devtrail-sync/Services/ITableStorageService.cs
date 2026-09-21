using devtrail_sync.Models.TableEntities;

namespace devtrail_sync.Services;

public interface ITableStorageService
{
    Task<int> UpsertRepositoryData(RepositoryEntity repositoryEntity);
}