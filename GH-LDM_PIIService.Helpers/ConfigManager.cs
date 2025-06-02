using Microsoft.Extensions.Configuration;
using System;
using GH_LDM_PIIService.Entities.Request;

namespace GH_LDM_PIIService.Helpers
{
    public class ConfigManager
    {
        private readonly IConfiguration _configuration;
        private readonly File_Logger _logger = File_Logger.GetInstance("ConfigManager");

        public ConfigManager(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            try
            {
                LogPath = GetValue<string>("Logging:LogPath");
                ConnectionString = GetValue<string>("ConnectionString", isEncrypted: false);

                ClientId = GetValue<string>("AuthService:ClientId");
                ClientSecret = GetValue<string>("AuthService:ClientSecret");
                AuthEndpoint = GetValue<string>("AuthService:Endpoint");
                GrantType = GetValue<string>("AuthService:GrantType");
                ClientAuthenticationMethod = GetValue<string>("AuthService:ClientAuthenticationMethod");

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
        public string GrantType { get; }
        public string ClientAuthenticationMethod { get; }
        public string PdfEndpoint { get; }
        public int IntervalMinutes { get; }

        public AuthRequestDto GetAuthRequestDto()
        {
            return new AuthRequestDto
            {
                GrantType = this.GrantType,
                ClientId = this.ClientId,
                ClientSecret = this.ClientSecret,
                ClientAuthenticationMethod = this.ClientAuthenticationMethod
            };
        }

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
