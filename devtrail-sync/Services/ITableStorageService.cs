using devtrail_sync.Models.GitHub;

namespace devtrail_sync.Services;

public interface ITableStorageService
{
    Task<int> UpsertRepositoryData(RepositorySnapshot repositorySnapshot);
}