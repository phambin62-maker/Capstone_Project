using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    public class LocationViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("locationName")]
        public string? LocationName { get; set; }
    }

    public class LocationsResponse
    {

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("locations")]
        public LocationsResponse2 Locations { get; set; } = new LocationsResponse2();
    }

    public class LocationsResponse2
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public List<LocationViewModel> Data { get; set; } = new List<LocationViewModel>();
    }
}
