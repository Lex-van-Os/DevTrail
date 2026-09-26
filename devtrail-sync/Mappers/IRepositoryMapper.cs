using devtrail_core.Models.TableEntities;

using devtrail_sync.Models.GitHub;

namespace devtrail_sync.Mappers;

public interface IRepositoryMapper
{
    RepositoryEntity Map(RepositorySnapshot repositorySnapshot);
}