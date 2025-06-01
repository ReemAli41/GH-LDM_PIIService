using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH_LDM_PIIService.Helpers
{
    public class ConfigManager
    {
        private readonly IConfiguration _configuration;
        private readonly File_Logger _logger = File_Logger.GetInstance("ConfigManager");

        public ConfigManager(IConfiguration configuration)
        {
            _configuration = configuration;

            try
            {
                LogPath = GetValue<string>("LogPath");
                ConnectionString = GetValue<string>("ConnectionString", isEncrypted: true);
                ClientId = GetValue<string>("ClientId");
                ClientSecret = GetValue<string>("ClientSecret");
                AuthEndpoint = GetValue<string>("AuthEndpoint");
                PdfEndpoint = GetValue<string>("PdfEndpoint");
                IntervalMinutes = GetValue<int>("IntervalMinutes");

                _logger.WriteToLogFile(ActionTypeEnum.Information, "Configuration loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.WriteToLogFile(ex, "Failed to load configuration.");
                throw;
            }
        }

        public string LogPath { get; }
        public string ConnectionString { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string AuthEndpoint { get; }
        public string PdfEndpoint { get; }
        public int IntervalMinutes { get; }

        private T GetValue<T>(string key, bool isEncrypted = false)
        {
            var value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.WriteToLogFile(ActionTypeEnum.Exception, $"Configuration key '{key}' is missing or empty.");
                throw new Exception($"Configuration key '{key}' is missing or empty.");
            }

            if (isEncrypted)
            {
                // value = Decrypt(value);
                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Configuration key '{key}' is encrypted (decryption logic not implemented).");
            }

            _logger.WriteToLogFile(ActionTypeEnum.Information, $"Configuration key '{key}' loaded.");
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
