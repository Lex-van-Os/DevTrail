using Azure.Data.Tables;

using devtrail_sync.Models.TableEntities;

namespace devtrail_sync.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageService(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient;
    }

    public async Task UpsertRepositoryData(RepositoryEntity repositoryEntity)
    {
        TableClient table = _tableServiceClient.GetTableClient("Repositories");
        await table.CreateIfNotExistsAsync();

        await table.UpsertEntityAsync(repositoryEntity, TableUpdateMode.Replace);
    }

    public async Task UpsertSolvedChallengeData(SolvedChallengeEntity solvedChallengeEntity)
    {
        TableClient table = _tableServiceClient.GetTableClient("SolvedChallenges");
        await table.CreateIfNotExistsAsync();

        await table.UpsertEntityAsync(solvedChallengeEntity, TableUpdateMode.Replace);
    }
}