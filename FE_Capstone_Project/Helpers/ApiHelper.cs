using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

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

        /// <summary>
        /// Lấy token từ session
        /// </summary>
        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");
        }

        /// <summary>
        /// Tạo HttpRequestMessage với Authorization header
        /// </summary>
        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, endpoint);
            
            // Thêm token vào header nếu có
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
                
                // Xử lý 401 Unauthorized
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearSession();
                    throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                // Xử lý 403 Forbidden
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền truy cập tài nguyên này.");
                }

                if (!response.IsSuccessStatusCode)
                    return default;

                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw để controller xử lý
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occurred while making GET request to " + ex);
                return default;
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
                
                // Xử lý 401 Unauthorized
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearSession();
                    throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                // Xử lý 403 Forbidden
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
                }

                if(!response.IsSuccessStatusCode)
                    return default;

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw để controller xử lý
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var request = CreateRequest(HttpMethod.Delete, endpoint);
                var response = await _httpClient.SendAsync(request);
                
                // Xử lý 401 Unauthorized
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearSession();
                    throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                // Xử lý 403 Forbidden
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw để controller xử lý
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Clear session khi token hết hạn
        /// </summary>
        private void ClearSession()
        {
            _httpContextAccessor.HttpContext?.Session?.Clear();
        }
    }
}
