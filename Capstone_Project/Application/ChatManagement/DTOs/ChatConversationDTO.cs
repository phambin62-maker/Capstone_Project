using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Application.ChatManagement.DTOs
{
    public class ChatConversationDTO
    {
        [JsonPropertyName("customerId")]
        public int CustomerId { get; set; }

        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("customerEmail")]
        public string? CustomerEmail { get; set; }

        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("staffName")]
        public string? StaffName { get; set; }

        [JsonPropertyName("lastMessage")]
        public string? LastMessage { get; set; }

        [JsonPropertyName("lastMessageDate")]
        public DateTime? LastMessageDate { get; set; }

        [JsonPropertyName("unreadCount")]
        public int UnreadCount { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatDTO> Messages { get; set; } = new List<ChatDTO>();
    }
}

