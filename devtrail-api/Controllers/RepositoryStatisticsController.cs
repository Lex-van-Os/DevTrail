using Microsoft.AspNetCore.Mvc;

namespace devtrail_api.Controllers;

[Route("api")]
[Produces("application/json")]
[ApiController]
public class RepositoryStatisticsController : ControllerBase
{
    public RepositoryStatisticsController()
    {
        
    }

    public async Task<IActionResult> Get()
    {
        return Ok();
    }
}