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
        [JsonPropertyName("data")]
        public List<LocationViewModel> Data { get; set; } = new List<LocationViewModel>();
    }
}
