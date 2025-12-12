using BE_Capstone_Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FE_Capstone_Project.Models
{
    public class CancelConditionDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int MinDaysBeforeTrip { get; set; }
        public int RefundPercent { get; set; }
        public DateTime CreatedDate { get; set; }
        public CancelStatus CancelStatus { get; set; }
    }

    public class CancelConditionCreateRequest
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Minimum days is required")]
        [Range(0, 255, ErrorMessage = "Days must be between 0 and 255")]
        public int MinDaysBeforeTrip { get; set; }

        [Required(ErrorMessage = "Refund percent is required")]
        [Range(0, 100, ErrorMessage = "Refund percent must be between 0 and 100")]
        public int RefundPercent { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public CancelStatus CancelStatus { get; set; }
    }

    public class CancelConditionUpdateRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Minimum days is required")]
        [Range(0, 255, ErrorMessage = "Days must be between 0 and 255")]
        public int MinDaysBeforeTrip { get; set; }

        [Required(ErrorMessage = "Refund percent is required")]
        [Range(0, 100, ErrorMessage = "Refund percent must be between 0 and 100")]
        public int RefundPercent { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public CancelStatus CancelStatus { get; set; }
    }
    public class PagedResultDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
