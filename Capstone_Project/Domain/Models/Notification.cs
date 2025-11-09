using BE_Capstone_Project.Domain.Enums;
using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class Notification
{
    // ĐÃ SỬA: Khớp với DbContext (chữ 'i' thường)
    public int Id { get; set; }

    // ĐÃ XÓA: Tất cả các thuộc tính 'Id' hoặc 'ID' bị trùng lặp

    // ĐÃ SỬA: Khớp với DbContext (chữ 'd' thường)
    public int UserId { get; set; }

    public string? Message { get; set; }

    public string? Title { get; set; }

    public DateTime? CreatedDate { get; set; }

    public NotificationType? NotificationType { get; set; }

    public virtual User User { get; set; } = null!;

    public bool IsRead { get; set; }
}