using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace devtrail_sync;

public class DevTrailSync
{
    [Function(nameof(DevTrailSync))]
    public void Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer, FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(DevTrailSync));

        logger.LogInformation("Timer executed");
    }
}