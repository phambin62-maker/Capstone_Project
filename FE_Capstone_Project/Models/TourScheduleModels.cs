using System.Text.Json;
using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    public class TourScheduleDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tourId")]
        public int TourId { get; set; }
        [JsonPropertyName("startLocation")]
        public string? StartLocation { get; set; }

        [JsonPropertyName("endLocation")]
        public string? EndLocation { get; set; }

        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? DepartureDate { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
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
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly DepartureDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [JsonPropertyName("arrivalDate")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly ArrivalDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
    }

    public class UpdateTourScheduleRequest
    {
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? DepartureDate { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? ArrivalDate { get; set; }


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
        public ApiResponse(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
        
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScheduleStatus
    {
        Cancelled = 0,
        Completed = 1,
        Ongoing = 2,
        Scheduled = 3,
    }
    public class TourResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
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

    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateOnly.ParseExact(reader.GetString()!, Format);

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Format));
    }

}