using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class TourImage
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public string? Image { get; set; }

    public virtual Tour Tour { get; set; } = null!;
}
