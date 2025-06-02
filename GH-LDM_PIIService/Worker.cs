using GH_LDM_PIIService.DSL;
using GH_LDM_PIIService.Helpers;

namespace GH_LDM_PIIService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ConfigManager _config;
        private readonly Timer _timer;
        private readonly UpdatePdfDSL _updatePdfDSL;
        private bool _isRunning;

        public Worker(ILogger<Worker> logger, ConfigManager config)
        {
            _logger = logger;
            _config = config;

            _updatePdfDSL = new UpdatePdfDSL(config);

            //Set up timer
            _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            File_Logger.GetLogFilePath_Event += () => _config.LogPath;
            //Start Timer
            var interval = TimeSpan.FromMinutes(_config.IntervalMinutes);
            _timer.Change(TimeSpan.Zero, interval);

            return Task.CompletedTask;
        }

        private async void OnTimerElapsed(object state)
        {
            if (_isRunning)
            {
                _logger.LogWarning("Previous task is still running, skipping this tick.");
                return;
            }

            try
            {
                _isRunning = true;
                _logger.LogInformation("Timer triggered at: {time}", DateTimeOffset.Now);

                await _updatePdfDSL.ProcessAttachmentAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in OnTimerElapsed.");
            }
            finally
            {
                _isRunning = false;
            }
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
