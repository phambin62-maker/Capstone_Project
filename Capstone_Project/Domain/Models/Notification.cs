using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Message { get; set; }

    public string? Title { get; set; }

    public DateTime? CreatedDate { get; set; }

    public byte? NotificationType { get; set; }

    public virtual User User { get; set; } = null!;
}
