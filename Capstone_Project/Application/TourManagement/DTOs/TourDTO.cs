using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.TourManagement.DTOs
{
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
