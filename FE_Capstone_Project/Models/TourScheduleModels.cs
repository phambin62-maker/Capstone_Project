using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    public class TourScheduleDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tourId")]
        public int TourId { get; set; }

        [JsonPropertyName("departureDate")]
        public DateOnly? DepartureDate { get; set; }

        [JsonPropertyName("arrivalDate")]
        public DateOnly? ArrivalDate { get; set; }

        [JsonPropertyName("scheduleStatus")]
        public ScheduleStatus? ScheduleStatus { get; set; }

        [JsonPropertyName("tourName")]
        public string? TourName { get; set; }
    }

    public class CreateTourScheduleRequest
    {
        [JsonPropertyName("tourId")]
        public int TourId { get; set; }

        [JsonPropertyName("departureDate")]
        public DateOnly DepartureDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [JsonPropertyName("arrivalDate")]
        public DateOnly ArrivalDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
    }

    public class UpdateTourScheduleRequest
    {
        [JsonPropertyName("departureDate")]
        public DateOnly DepartureDate { get; set; }

        [JsonPropertyName("arrivalDate")]
        public DateOnly ArrivalDate { get; set; }

        [JsonPropertyName("scheduleStatus")]
        public ScheduleStatus ScheduleStatus { get; set; }
    }

    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T Data { get; set; }
        public class TourResponse
        {
                [JsonPropertyName("id")]
                public int Id { get; set; }

                [JsonPropertyName("name")]
                public string Name { get; set; } = string.Empty;
        }
        public ApiResponse(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
        
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScheduleStatus : byte
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }

    public class TourScheduleListResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public List<TourScheduleDTO> Data { get; set; } = new List<TourScheduleDTO>();
    }
}