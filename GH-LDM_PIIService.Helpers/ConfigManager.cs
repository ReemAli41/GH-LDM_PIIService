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

        public ConfigManager(IConfiguration configuration)
        {
            _configuration = configuration;

            LogPath = GetValue<string>("LogPath");
            ConnectionString = GetValue<string>("ConnectionString", true);
            ClientId = GetValue<string>("ClientId");
            ClientSecret = GetValue<string>("ClientSecret");
            AuthEndpoint = GetValue<string>("AuthEndpoint");
            PdfEndpoint = GetValue<string>("PdfEndpoint");
            IntervalMinutes = GetValue<int>("IntervalMinutes");
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
            if (isEncrypted)
            {
                //value = Decrypt(value);
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
