using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Application.Company.DTOs
{
    public class FeatureDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("delay")]
        public int Delay { get; set; } = 200;

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; } = 0;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;
    }
}

