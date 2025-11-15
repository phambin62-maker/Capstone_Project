namespace FE_Capstone_Project.Models
{
    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public string? StaffName { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Message { get; set; }
        public int? ChatType { get; set; }
        public DateTime? SentDate { get; set; }
        public int? SenderId { get; set; }
    }

    public class ChatConversationViewModel
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public int StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>();
    }

    public class CreateChatViewModel
    {
        public int StaffId { get; set; }
        public int CustomerId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ChatType { get; set; } // Sẽ được set tự động bởi backend dựa trên role
        public int SenderId { get; set; }
    }
}

