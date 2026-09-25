using devtrail_api.Mappers;
using devtrail_api.Models;

using devtrail_core.Models.TableEntities;
using devtrail_core.Services;

using Microsoft.AspNetCore.Mvc;

namespace devtrail_api.Controllers;

[Route("api")]
[Produces("application/json")]
[ApiController]
public class RepositoryStatisticsController : ControllerBase
{
    private readonly IRepositoryStatisticsMapper _repositoryStatisticsMapper;
    private readonly ITableStorageService _tableStorageService;

    public RepositoryStatisticsController(ITableStorageService tableStorageService,
        IRepositoryStatisticsMapper repositoryStatisticsMapper)
    {
        _tableStorageService = tableStorageService;
        _repositoryStatisticsMapper = repositoryStatisticsMapper;
    }

    [EndpointName("GetRepositoryStatistics")]
    [EndpointSummary("Retrieves repository statistics.")]
    [HttpGet("repositoryStatistics")]
    public async Task<IActionResult> Get()
    {
        List<RepositoryEntity> repositoryEntities = await _tableStorageService.GetRepositoryData();

        List<SolvedChallengeEntity> solvedChallengeEntities = await _tableStorageService.GetSolvedChallengeData();

        List<RepositoryStatisticsResponse> response =
            _repositoryStatisticsMapper.Map(repositoryEntities, solvedChallengeEntities);

        return Ok(response);
    }
}