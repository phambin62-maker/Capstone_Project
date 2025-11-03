using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScheduleStatus : byte
    {
        Cancelled = 0,
        Completed = 1,
        Ongoing = 2,
        Scheduled = 3,
    }
}
