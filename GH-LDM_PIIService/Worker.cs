using GH_LDM_PIIService.Helpers;

namespace GH_LDM_PIIService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        //private readonly ConfigManager _config;

        public Worker(ILogger<Worker> logger /*,ConfigManager config*/)
        {
            _logger = logger;
          //  File_Logger.GetLogFilePath_Event += () => _config.LogPath;
           // _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
