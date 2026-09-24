using Azure.Data.Tables;

using Microsoft.AspNetCore.Mvc;

namespace devtrail_api.Controllers;

[Route("api")]
[Produces("application/json")]
[ApiController]
public class RepositoryStatisticsController : ControllerBase
{
    private readonly TableServiceClient _tableServiceClient;

    public RepositoryStatisticsController(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient;
    }

    public async Task<IActionResult> Get()
    {
        
        return Ok();
    }
}