using BE_Capstone_Project.Domain.Models;
using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    public class TourViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("duration")]
        public byte Duration { get; set; }

        [JsonPropertyName("startLocationId")]
        public int StartLocationId { get; set; }

        [JsonPropertyName("endLocationId")]
        public int EndLocationId { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("cancelConditionId")]
        public int CancelConditionId { get; set; }

        [JsonPropertyName("childDiscount")]
        public decimal? ChildDiscount { get; set; }

        [JsonPropertyName("groupDiscount")]
        public decimal? GroupDiscount { get; set; }

        [JsonPropertyName("groupNumber")]
        public byte? GroupNumber { get; set; }

        [JsonPropertyName("minSeats")]
        public short? MinSeats { get; set; }

        [JsonPropertyName("maxSeats")]
        public short? MaxSeats { get; set; }

        [JsonPropertyName("tourStatus")]
        public bool TourStatus { get; set; }

        [JsonPropertyName("category")]
        public TourCategory Category { get; set; } = new TourCategory();

        [JsonPropertyName("tourImages")]
        public List<TourImage> TourImages { get; set; } = new List<TourImage>();

        [JsonPropertyName("reviews")]
        public List<Review> Reviews { get; set; } = new List<Review>();

        [JsonPropertyName("startLocation")]
        public Location StartLocation { get; set; } = new Location();

        [JsonPropertyName("endLocation")]
        public Location EndLocation { get; set; } = new Location();
    }

    public class TourListResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("tours")]
        public List<TourViewModel> Tours { get; set; } = new List<TourViewModel>();
    }

    public class TourDetailResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("tour")]
        public TourViewModel? Tour { get; set; }

        [JsonPropertyName("canComment")]
        public bool CanComment { get; set; }
    }

    public class TourCountResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("tourCount")]
        public int TourCount { get; set; }
    }
}
