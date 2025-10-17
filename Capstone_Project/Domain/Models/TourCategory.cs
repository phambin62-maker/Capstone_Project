using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class TourCategory
{
    public int Id { get; set; }

    public string? CategoryName { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
