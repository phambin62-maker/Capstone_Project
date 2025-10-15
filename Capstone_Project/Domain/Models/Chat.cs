using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class Chat
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public int CustomerId { get; set; }

    public string? Message { get; set; }

    public byte? ChatType { get; set; }

    public DateTime? SentDate { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;
}
