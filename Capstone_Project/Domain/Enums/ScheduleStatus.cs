using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScheduleStatus : byte
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }
}
