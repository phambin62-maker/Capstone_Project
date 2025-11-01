using System.ComponentModel.DataAnnotations;

namespace FE_Capstone_Project.Models
{
    public class TourCreateModel
    {
        [Required(ErrorMessage = "Tên tour là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tour không được vượt quá 100 ký tự")]
        public string Name { get; set; } = "Tour mới";

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; } = "Mô tả tour";

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000, 1000000000, ErrorMessage = "Giá phải từ 1,000 VND đến 1,000,000,000 VND")]
        public decimal Price { get; set; } = 1000000;

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
        public decimal ChildDiscount { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá nhóm phải từ 0-100%")]
        public decimal GroupDiscount { get; set; }

        [Range(1, 100, ErrorMessage = "Số người nhóm phải từ 1-100")]
        public byte GroupNumber { get; set; } = 5;

        [Range(1, 100, ErrorMessage = "Số chỗ tối thiểu phải từ 1-100")]
        public short MinSeats { get; set; } = 10;

        [Range(1, 100, ErrorMessage = "Số chỗ tối đa phải từ 1-100")]
        public short MaxSeats { get; set; } = 30;

        public List<IFormFile>? Images { get; set; }
    }

    public class TourEditModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tour là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tour không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000, 1000000000, ErrorMessage = "Giá phải từ 1,000 VND đến 1,000,000,000 VND")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Thời lượng là bắt buộc")]
        public byte Duration { get; set; }

        [Required(ErrorMessage = "Điểm xuất phát là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn điểm xuất phát")]
        public int StartLocationId { get; set; }

        [Required(ErrorMessage = "Điểm đến là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn điểm đến")]
        public int EndLocationId { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Điều kiện hủy là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn điều kiện hủy")]
        public int CancelConditionId { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá trẻ em phải từ 0-100%")]
        public decimal ChildDiscount { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá nhóm phải từ 0-100%")]
        public decimal GroupDiscount { get; set; }

        [Range(1, 100, ErrorMessage = "Số người nhóm phải từ 1-100")]
        public byte GroupNumber { get; set; } = 5;

        [Range(1, 100, ErrorMessage = "Số chỗ tối thiểu phải từ 1-100")]
        public short MinSeats { get; set; } = 10;

        [Range(1, 100, ErrorMessage = "Số chỗ tối đa phải từ 1-100")]
        public short MaxSeats { get; set; } = 30;

        public List<IFormFile>? Images { get; set; }
    }

    public class TourDTO
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Duration { get; set; }
        public int StartLocationId { get; set; }
        public int EndLocationId { get; set; }
        public int CategoryId { get; set; }
        public int CancelConditionId { get; set; }
        public decimal? ChildDiscount { get; set; }
        public decimal? GroupDiscount { get; set; }
        public byte? GroupNumber { get; set; }
        public short? MinSeats { get; set; }
        public short? MaxSeats { get; set; }
    }
}