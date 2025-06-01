using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace GH_LDM_PIIService.Helpers
{
    public static class HttpHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
        {
            UseCookies = false,
            ConnectTimeout = TimeSpan.FromMinutes(5),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        })
        {
            Timeout = TimeSpan.FromMinutes(5)
        };

        private static readonly JsonSerializerOptions _defaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        };

        private static readonly File_Logger _logger = File_Logger.GetInstance("HttpHelper");

        public static async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
            string url,
            TRequest body,
            string bearerToken = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                if (!string.IsNullOrEmpty(bearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    _logger.WriteToLogFile(ActionTypeEnum.Information, $"Bearer token added to request.");
                }

                request.Content = JsonContent.Create(body, options: _defaultJsonOptions);
                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Sending POST JSON to {url}.");

                var response = await _httpClient.SendAsync(request);

                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Received response with status code {response.StatusCode} from {url}.");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<TResponse>(_defaultJsonOptions);
                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Deserialized response successfully.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.WriteToLogFile(ex, $"Error occurred while sending JSON request to {url}");
                throw;
            }
        }

        public static async Task<TResponse> PostFormUrlEncodedAsync<TRequest, TResponse>(
            string url,
            TRequest body,
            string clientId,
            string clientSecret)
            where TRequest : class
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                var formData = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["client_authentication_method"] = "client_secret_post"
                };

                request.Content = new FormUrlEncodedContent(formData);
                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Sending form-url-encoded POST to {url}.");

                var response = await _httpClient.SendAsync(request);
                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Received response with status code {response.StatusCode} from {url}.");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<TResponse>(_defaultJsonOptions);
                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Form-url-encoded response deserialized successfully.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.WriteToLogFile(ex, $"Error occurred during form-url-encoded POST to {url}");
                throw;
            }
        }
    }
}

