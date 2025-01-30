using Serilog;

namespace Helicopters_Russia
{
    public class Worker(/*ILogger<Worker> logger*/) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Log.Information("Worker running at: {time}", DateTimeOffset.Now);
                //logger.LogInformation();
                await Task.Delay(100000, stoppingToken);
            }
        }
    }
}
