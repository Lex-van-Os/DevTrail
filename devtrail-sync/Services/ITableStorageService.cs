using devtrail_sync.Models;

namespace devtrail_sync.Services;

public interface ITableStorageService
{
    Task<int> UpsertRepositoryData(RepositorySnapshot repositorySnapshot);
}