using System;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using GH_LDM_PIIService.Helpers;
using GH_LDM_PIIService.DSL;

namespace GH_LDM_PIIService
{
    public class TimerDSL : IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private readonly UpdatePdfDSL _updatePdfDSL;
        private readonly File_Logger _logger = File_Logger.GetInstance("TimerDSL");
        private readonly ConfigManager _config;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isProcessing = false;

        public TimerDSL(ConfigManager config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _updatePdfDSL = new UpdatePdfDSL(_config);

            int intervalInMinutes = _config.IntervalMinutes;
            double intervalInMilliseconds = intervalInMinutes * 60 * 1000;

            _timer = new System.Timers.Timer(intervalInMilliseconds);
            _timer.Elapsed += async (sender, args) => await OnElapsedAsync();
            _timer.AutoReset = true;

            _logger.WriteToLogFile(ActionTypeEnum.Information, $"TimerDSL initialized with interval {intervalInMinutes} minutes.");
        }

        public void Start()
        {
            _logger.WriteToLogFile(ActionTypeEnum.Information, "Timer started.");
            _timer.Start();
        }

        public void Stop()
        {
            _logger.WriteToLogFile(ActionTypeEnum.Information, "Timer stopped.");
            _timer.Stop();
        }

        private async Task OnElapsedAsync()
        {
            if (_isProcessing)
            {
                _logger.WriteToLogFile(ActionTypeEnum.Information, "Previous task still running. Skipping this tick.");
                return;
            }

            try
            {
                _isProcessing = true;
                _logger.WriteToLogFile(ActionTypeEnum.Information, "Timer ticked. Starting attachment processing.");
                await _updatePdfDSL.ProcessAttachmentAsync(_cts.Token);
                _logger.WriteToLogFile(ActionTypeEnum.Information, "Attachment processing completed.");
            }
            catch (Exception ex)
            {
                _logger.WriteToLogFile(ex, "Error during timer elapsed event.");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
            _logger.WriteToLogFile(ActionTypeEnum.Information, "TimerDSL disposed.");
        }
    }
}
