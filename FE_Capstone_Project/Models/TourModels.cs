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

    // User model cho View
    public class UserViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    // TourImage model cho View
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
        [Required(ErrorMessage = "Tên tour là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tour không được vượt quá 100 ký tự")]
        public string Name { get; set; } = "Tour Hà Nội - Sapa";

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; } = "Tour du lịch Hà Nội Sapa 3 ngày 2 đêm";

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000, 1000000000, ErrorMessage = "Giá phải từ 1,000 VND đến 1,000,000,000 VND")]
        public decimal Price { get; set; } = 2500000;

        [Required(ErrorMessage = "Thời lượng là bắt buộc")]
        public byte Duration { get; set; } = 3;

        [Required(ErrorMessage = "Điểm xuất phát là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn điểm xuất phát")]
        public int StartLocationId { get; set; } = 1;

        [Required(ErrorMessage = "Điểm đến là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn điểm đến")]
        public int EndLocationId { get; set; } = 2;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn danh mục")]
        public int CategoryId { get; set; } = 1;

        [Required(ErrorMessage = "Điều kiện hủy là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn điều kiện hủy")]
        public int CancelConditionId { get; set; } = 1;

        [Range(0, 100, ErrorMessage = "Giảm giá trẻ em phải từ 0-100%")]
        public decimal ChildDiscount { get; set; } = 10;

        [Range(0, 100, ErrorMessage = "Giảm giá nhóm phải từ 0-100%")]
        public decimal GroupDiscount { get; set; } = 15;

        [Range(1, 100, ErrorMessage = "Số người nhóm phải từ 1-100")]
        public byte GroupNumber { get; set; } = 8;

        [Range(1, 100, ErrorMessage = "Số chỗ tối thiểu phải từ 1-100")]
        public short MinSeats { get; set; } = 10;

        [Range(1, 100, ErrorMessage = "Số chỗ tối đa phải từ 1-100")]
        public short MaxSeats { get; set; } = 30;

        [Required(ErrorMessage = "Hình ảnh tour là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 hình ảnh")]
        [MaxLength(10, ErrorMessage = "Không được upload quá 10 hình ảnh")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }

    public class TourEditModel : TourCreateModel
    {
        public int Id { get; set; }

        new public List<IFormFile>? Images { get; set; }

        public List<string> ExistingImages { get; set; } = new List<string>();
    }

    
}