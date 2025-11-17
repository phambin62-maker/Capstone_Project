using System.Text.Json;
using BE_Capstone_Project.Domain.Models;

namespace FE_Capstone_Project.Services
{
    public class DataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DataService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7160/api/");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async Task<(bool Success, T Data, string Error)> CallApiAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return (true, data, "");
                }

                return (false, default, content);
            }
            catch (Exception ex)
            {
                return (false, default, ex.Message);
            }
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            var (success, data, _) = await CallApiAsync<List<Location>>("Locations");
            return success ? data ?? new List<Location>() : new List<Location>();
        }


        public async Task<List<TourCategory>> GetAllCategoriesAsync()
        {
            var (success, data, _) = await CallApiAsync<List<TourCategory>>("TourCategories");
            return success ? data ?? new List<TourCategory>() : new List<TourCategory>();
        }

        public async Task<List<CancelCondition>> GetAllCancelConditionsAsync()
        {
            var (success, data, _) = await CallApiAsync<List<CancelCondition>>("CancelCondition");
            return success ? data ?? new List<CancelCondition>() : new List<CancelCondition>();
        }

        public async Task<string> GetNameByIdAsync(string endpoint)
        {
            var (success, response, _) = await CallApiAsync<JsonElement>(endpoint);
            if (!success) return "Không tìm thấy";

            if (response.TryGetProperty("locationName", out var loc))
                return loc.GetString() ?? "Không có tên";
            if (response.TryGetProperty("categoryName", out var cat))
                return cat.GetString() ?? "Không có tên";
            if (response.TryGetProperty("title", out var title))
                return title.GetString() ?? "Không có tên";
            if (response.TryGetProperty("name", out var name))
                return name.GetString() ?? "Không có tên";

            return "Không có tên";
        }
    }
}
