using devtrail_sync.Models.GitHub;
using devtrail_sync.Models.TableEntities;

namespace devtrail_sync.Mappers;

public interface IRepositoryMapper
{
    RepositoryEntity Map(RepositorySnapshot repositorySnapshot);
}
