using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;

namespace FE_Capstone_Project.Helpers
{
    public class ApiHelper
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiHelper(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine($"[ApiHelper] WARNING: No token found for request to {endpoint}");
            }

            if (content != null)
            {
                request.Content = content;
            }

            return request;
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var request = CreateRequest(HttpMethod.Get, endpoint);
                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearSession();
                    throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền truy cập tài nguyên này.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API request failed at GET {endpoint}. Status: {response.StatusCode}.");
                }

                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while making GET request: {ex.Message}");
                throw new Exception($"ApiHelper GET Error: {ex.Message}", ex);
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = CreateRequest(HttpMethod.Post, endpoint, content);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearSession();
                    throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API request failed at POST {endpoint}. Status: {response.StatusCode}.");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while making POST request: {ex.Message}");
                throw new Exception($"ApiHelper POST Error: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var request = CreateRequest(HttpMethod.Delete, endpoint);
                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearSession();
                    throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API request failed at DELETE {endpoint}. Status: {response.StatusCode}.");
                }
                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while making DELETE request: {ex.Message}");
                throw new Exception($"ApiHelper DELETE Error: {ex.Message}", ex);
            }
        }
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = CreateRequest(HttpMethod.Put, endpoint, content);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearSession();
                    throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"PUT request failed. Status: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"API request failed at PUT {endpoint}. Status: {response.StatusCode}.");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"PUT response: {responseJson}");

                return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while making PUT request: {ex.Message}");
                throw new Exception($"ApiHelper PUT Error: {ex.Message}", ex);
            }
        }

        private void ClearSession()
        {
            _httpContextAccessor.HttpContext?.Session?.Clear();
        }
    }
}