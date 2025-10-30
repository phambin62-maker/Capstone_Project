using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.TourManagement.DTOs
{
    public class TourScheduleDTO
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public DateOnly? DepartureDate { get; set; }
        public DateOnly? ArrivalDate { get; set; }
        public ScheduleStatus? ScheduleStatus { get; set; }
        public string? TourName { get; set; }
    }

    public class CreateTourScheduleRequest
    {
        public int TourId { get; set; }
        public DateOnly DepartureDate { get; set; }
        public DateOnly ArrivalDate { get; set; }
    }

    public class UpdateTourScheduleRequest
    {
        public DateOnly DepartureDate { get; set; }
        public DateOnly ArrivalDate { get; set; }
        public ScheduleStatus ScheduleStatus { get; set; }
    }
}
