using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Application.ChatManagement.DTOs
{
    public class CreateChatDTO
    {
        [Required(ErrorMessage = "StaffId is required")]
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "CustomerId is required")]
        [JsonPropertyName("customerId")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("chatType")]
        public int? ChatType { get; set; } // Sẽ được set tự động bởi backend dựa trên role

        [JsonPropertyName("senderId")]
        public int SenderId { get; set; } // ID của người gửi
    }
}

