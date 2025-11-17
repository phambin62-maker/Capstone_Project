// Models/TourModels.cs
using BE_Capstone_Project.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    // Tour View Models
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

    // User model for View
    public class UserViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    // TourImage model for View
    public class TourImageViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("tourId")]
        public int TourId { get; set; }
    }

    // API Response Models
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
        [JsonPropertyName("canComment")]
        public bool CanComment { get; set; }
        [JsonPropertyName("tour")]
        public TourViewModel? Tour { get; set; }
    }

    public class TourCountResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("tourCount")]
        public int TourCount { get; set; }
    }

    public class TourStatusResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("newStatus")]
        public bool NewStatus { get; set; }
    }

    public class TourDetailModel : TourViewModel
    {
        public string StartLocationName { get; set; } = string.Empty;
        public string EndLocationName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CancelConditionName { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
    }

    public class TourCreateModel
    {
        [Required(ErrorMessage = "Tour name is required")]
        [StringLength(100, ErrorMessage = "Tour name cannot exceed 100 characters")]
        public string Name { get; set; } = "Hanoi - Sapa Tour";

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "3 days 2 nights Hanoi Sapa tour";

        [Required(ErrorMessage = "Price is required")]
        [Range(1000, 1000000000, ErrorMessage = "Price must be from 1,000 VND to 1,000,000,000 VND")]
        public decimal Price { get; set; } = 2500000;

        [Required(ErrorMessage = "Duration is required")]
        public byte Duration { get; set; } = 3;

        [Required(ErrorMessage = "Start location is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Select start location")]
        public int StartLocationId { get; set; } = 1;

        [Required(ErrorMessage = "End location is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Select end location")]
        public int EndLocationId { get; set; } = 2;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Select category")]
        public int CategoryId { get; set; } = 1;

        [Required(ErrorMessage = "Cancellation condition is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Select cancellation condition")]
        public int CancelConditionId { get; set; } = 1;

        [Range(0, 100, ErrorMessage = "Child discount must be 0-100%")]
        public decimal ChildDiscount { get; set; } = 10;

        [Range(0, 100, ErrorMessage = "Group discount must be 0-100%")]
        public decimal GroupDiscount { get; set; } = 15;

        [Range(1, 100, ErrorMessage = "Group size must be 1-100")]
        public byte GroupNumber { get; set; } = 8;

        [Range(1, 100, ErrorMessage = "Minimum seats must be 1-100")]
        public short MinSeats { get; set; } = 10;

        [Range(1, 100, ErrorMessage = "Maximum seats must be 1-100")]
        public short MaxSeats { get; set; } = 30;

        [Required(ErrorMessage = "Tour images are required")]
        [MinLength(1, ErrorMessage = "Must have at least 1 image")]
        [MaxLength(10, ErrorMessage = "Cannot upload more than 10 images")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }

    public class TourEditModel : TourCreateModel
    {
        public int Id { get; set; }

        new public List<IFormFile>? Images { get; set; }

        public List<string> ExistingImages { get; set; } = new List<string>();
    }
}