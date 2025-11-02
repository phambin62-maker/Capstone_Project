using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    public class TourCategoryViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class TourCategoriesResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public List<TourCategoryViewModel> Data { get; set; } = new List<TourCategoryViewModel>();
    }
}
