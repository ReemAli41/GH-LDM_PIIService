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
                    }

                    request.Content = JsonContent.Create(body, options: _defaultJsonOptions);

                    _logger.WriteToLogFile(ActionTypeEnum.Information, $"Sending JSON request to {url}");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadFromJsonAsync<TResponse>(_defaultJsonOptions);
                }
                catch (Exception ex)
                {
                    _logger.WriteToLogFile(ActionTypeEnum.Exception, $"Error in PostJsonAsync: {ex.Message}");
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

                    _logger.WriteToLogFile(ActionTypeEnum.Information, $"Sending auth request to {url}");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadFromJsonAsync<TResponse>(_defaultJsonOptions);
                }
                catch (Exception ex)
                {
                    _logger.WriteToLogFile(ActionTypeEnum.Exception, $"Error in PostFormUrlEncodedAsync: {ex.Message}");
                    throw;
                }
            }
        }
}

