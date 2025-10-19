using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Tour.DTOs
{
    public class CreateTourDTO
    {
        // ====== Thông tin cơ bản ======
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Duration { get; set; }

        // ====== Khóa ngoại quan hệ ======
        public int StartLocationId { get; set; }
        public int EndLocationId { get; set; }
        public int CategoryId { get; set; }
        public int CancelConditionId { get; set; }

        // ====== Thông tin giảm giá và chỗ ngồi ======
        public decimal? ChildDiscount { get; set; }
        public decimal? GroupDiscount { get; set; }
        public byte? GroupNumber { get; set; }
        public short? MinSeats { get; set; }
        public short? MaxSeats { get; set; }
    }
}
