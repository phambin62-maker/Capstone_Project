using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class Location
{
    public int Id { get; set; }

    public string? LocationName { get; set; }

    public virtual ICollection<Tour> TourEndLocations { get; set; } = new List<Tour>();

    public virtual ICollection<Tour> TourStartLocations { get; set; } = new List<Tour>();
}
