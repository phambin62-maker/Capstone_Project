using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Application.ChatManagement.DTOs
{
    public class ChatDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("staffName")]
        public string? StaffName { get; set; }

        [JsonPropertyName("customerId")]
        public int CustomerId { get; set; }

        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("chatType")]
        public int? ChatType { get; set; }

        [JsonPropertyName("sentDate")]
        public DateTime? SentDate { get; set; }

        [JsonPropertyName("senderId")]
        public int? SenderId { get; set; } // ID của người gửi (CustomerId hoặc StaffId)
    }
    public record UserInfo(string Name, string Email);
}

