using System;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using GH_LDM_PIIService.Helpers;
using GH_LDM_PIIService.DSL;

namespace GH_LDM_PIIService
{
    //public class TimerDSL : IDisposable
    //{
    //    private readonly System.Timers.Timer _timer;
    //    private readonly UpdatePdfDSL _updatePdfDSL;
    //    private readonly File_Logger _logger = File_Logger.GetInstance("TimerDSL");
    //    private readonly ConfigManager _config;
    //    private CancellationTokenSource _cts = new CancellationTokenSource();
    //    private bool _isProcessing = false;

    //    public TimerDSL(ConfigManager config)
    //    {
    //        _config = config ?? throw new ArgumentNullException(nameof(config));
    //        _logger = File_Logger.GetInstance("TimerDSL");

    //        _updatePdfDSL = new UpdatePdfDSL(_config);

    //        int intervalInMinutes = _config.IntervalMinutes;
    //        double intervalInMilliseconds = intervalInMinutes * 60 * 1000;

    //        _timer = new System.Timers.Timer(intervalInMilliseconds);
    //        _timer.Elapsed += async (sender, args) => await OnElapsedAsync();
    //        _timer.AutoReset = true;

    //        _logger.WriteToLogFile(ActionTypeEnum.Information, $"TimerDSL initialized with interval {intervalInMinutes} minutes.");
    //    }

    //    public void Start()
    //    {
    //        _logger.WriteToLogFile(ActionTypeEnum.Information, "Timer started.");
    //        _timer.Start();
    //    }

    //    public void Stop()
    //    {
    //        _logger.WriteToLogFile(ActionTypeEnum.Information, "Timer stopped.");
    //        _timer.Stop();
    //    }

    //    private async Task OnElapsedAsync()
    //    {
    //        if (_isProcessing)
    //        {
    //            _logger.WriteToLogFile(ActionTypeEnum.Information, "Previous task still running. Skipping this tick.");
    //            return;
    //        }

    //        try
    //        {
    //            _isProcessing = true;
    //            _logger.WriteToLogFile(ActionTypeEnum.Information, "Timer ticked. Starting attachment processing.");
    //            await _updatePdfDSL.ProcessAttachmentAsync(_cts.Token);
    //            _logger.WriteToLogFile(ActionTypeEnum.Information, "Attachment processing completed.");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.WriteToLogFile(ex, "Error during timer elapsed event.");
    //        }
    //        finally
    //        {
    //            _isProcessing = false;
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        _timer?.Stop();
    //        _timer?.Dispose();
    //        _cts?.Cancel();
    //        _cts?.Dispose();
    //        _logger.WriteToLogFile(ActionTypeEnum.Information, "TimerDSL disposed.");
    //    }
    //}
    public class TimerDSL : IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private readonly UpdatePdfDSL _updatePdfDSL;
        private readonly File_Logger _logger;
        private readonly ConfigManager _config;
        private CancellationTokenSource _cts;
        private bool _isProcessing = false;

        public TimerDSL(ConfigManager config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = File_Logger.GetInstance("TimerDSL"); // Single initialization
            _updatePdfDSL = new UpdatePdfDSL(_config);
            _cts = new CancellationTokenSource();

            // Convert minutes to milliseconds
            _timer = new System.Timers.Timer(_config.IntervalMinutes * 60 * 1000)
            {
                AutoReset = true
            };
            _timer.Elapsed += OnTimerElapsed;

            _logger.WriteToLogFile(ActionTypeEnum.Information,
                $"Timer initialized with {_config.IntervalMinutes} minute interval");
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isProcessing)
            {
                _logger.WriteToLogFile(ActionTypeEnum.Information,
                    "Previous execution still in progress. Skipping.");
                return;
            }

            _isProcessing = true;
            try
            {
                _logger.WriteToLogFile(ActionTypeEnum.Information,
                    "Starting PDF processing cycle");

                await _updatePdfDSL.ProcessAttachmentAsync(_cts.Token);

                _logger.WriteToLogFile(ActionTypeEnum.Information,
                    "PDF processing completed successfully");
            }
            catch (Exception ex)
            {
                _logger.WriteToLogFile(ActionTypeEnum.Exception,
                    $"PDF processing failed: {ex}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void Start()
        {
            _timer.Start();
            _logger.WriteToLogFile(ActionTypeEnum.Action, "Timer started");
        }

        public void Stop()
        {
            _timer.Stop();
            _logger.WriteToLogFile(ActionTypeEnum.Action, "Timer stopped");
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _cts?.Cancel();
            _logger.WriteToLogFile(ActionTypeEnum.Information, "Timer resources released");
            GC.SuppressFinalize(this);
        }
    }
}
