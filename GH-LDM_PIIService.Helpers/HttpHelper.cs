using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using GH_LDM_PIIService.Entities.Request;
using GH_LDM_PIIService.Entities.Response;

namespace GH_LDM_PIIService.Helpers
{
    public class HttpHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigManager _config;
        private readonly File_Logger _logger = File_Logger.GetInstance("HttpHelper");

        private readonly JsonSerializerOptions _defaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        };

        public HttpHelper(ConfigManager config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _httpClient = new HttpClient(new SocketsHttpHandler
            {
                UseCookies = false,
                ConnectTimeout = TimeSpan.FromMinutes(5),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            })
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public async Task<PdfUpdateResponseDto> PostJsonAsync(
            PdfUpdateRequestDto body,
            string bearerToken = null)
        {
            string url = _config.PdfEndpoint ?? throw new InvalidOperationException("PdfEndpoint is not configured.");

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
                int statusCode = (int)response.StatusCode;

                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Received response with status code {statusCode} from {url}.");

                if (statusCode >= 200 && statusCode < 400)
                {
                    var result = await response.Content.ReadFromJsonAsync<PdfUpdateResponseDto>(_defaultJsonOptions);
                    _logger.WriteToLogFile(ActionTypeEnum.Information, $"Deserialized response successfully.");
                    return result;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    _logger.WriteToLogFile(ActionTypeEnum.Error, $"Request to {url} failed with status {statusCode}: {error}");
                    throw new HttpRequestException($"HTTP Error {statusCode}: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.WriteToLogFile(ex, $"Error occurred while sending JSON request to {url}");
                throw;
            }
        }

        public async Task<AuthResponseDto> PostFormUrlEncodedAsync()
        {
            string url = _config.AuthEndpoint ?? throw new InvalidOperationException("AuthEndpoint is not configured.");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                var formData = new Dictionary<string, string>
                {
                    ["grant_type"] = _config.GrantType,
                    ["client_id"] = _config.ClientId,
                    ["client_secret"] = _config.ClientSecret,
                    ["client_authentication_method"] = _config.ClientAuthenticationMethod
                };

                request.Content = new FormUrlEncodedContent(formData);
                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Sending form-url-encoded POST to {url}.");

                var response = await _httpClient.SendAsync(request);
                int statusCode = (int)response.StatusCode;

                _logger.WriteToLogFile(ActionTypeEnum.Information, $"Received response with status code {statusCode} from {url}.");

                if (statusCode >= 200 && statusCode < 400)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_defaultJsonOptions);
                    _logger.WriteToLogFile(ActionTypeEnum.Information, $"Form-url-encoded response deserialized successfully.");
                    return result;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    _logger.WriteToLogFile(ActionTypeEnum.Error, $"Form-url-encoded POST to {url} failed with status {statusCode}: {error}");
                    throw new HttpRequestException($"HTTP Error {statusCode}: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.WriteToLogFile(ex, $"Error occurred during form-url-encoded POST to {url}");
                throw;
            }
        }
    }
}
