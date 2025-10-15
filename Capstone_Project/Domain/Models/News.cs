using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class News
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Image { get; set; }

    public DateTime? CreatedDate { get; set; }

    public byte? NewsStatus { get; set; }

    public virtual User User { get; set; } = null!;
}
