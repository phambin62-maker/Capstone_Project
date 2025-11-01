using BE_Capstone_Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BE_Capstone_Project.Application.CancelConditions.DTOs
{
    public class CancelConditionDTO
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public byte? MinDaysBeforeTrip { get; set; }

        public byte? RefundPercent { get; set; }

        public DateTime? CreatedDate { get; set; }

        public CancelStatus? CancelStatus { get; set; }
    }

    public class CancelConditionCreateDTO
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "MinDaysBeforeTrip is required.")]
        [Range(0, 255, ErrorMessage = "MinDaysBeforeTrip must be between 0 and 255.")]
        public byte MinDaysBeforeTrip { get; set; }

        [Required(ErrorMessage = "RefundPercent is required.")]
        [Range(0, 100, ErrorMessage = "RefundPercent must be between 0 and 100.")]
        public byte RefundPercent { get; set; }

        [Required(ErrorMessage = "CancelStatus is required.")]
        public CancelStatus CancelStatus { get; set; }
    }

    public class CancelConditionUpdateDTO
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "MinDaysBeforeTrip is required.")]
        [Range(0, 255, ErrorMessage = "MinDaysBeforeTrip must be between 0 and 255.")]
        public byte MinDaysBeforeTrip { get; set; }

        [Required(ErrorMessage = "RefundPercent is required.")]
        [Range(0, 100, ErrorMessage = "RefundPercent must be between 0 and 100.")]
        public byte RefundPercent { get; set; }

        [Required(ErrorMessage = "CancelStatus is required.")]
        public CancelStatus CancelStatus { get; set; }
    }
}
