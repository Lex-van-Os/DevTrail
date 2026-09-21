using devtrail_sync.Models.GitHub;

namespace devtrail_sync.Services;

public class TableStorageService : ITableStorageService
{
    public async Task<int> UpsertRepositoryData(RepositorySnapshot repositorySnapshot)
    {
        return 1;
    }
}