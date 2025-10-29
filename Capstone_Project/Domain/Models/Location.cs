using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Domain.Models;

public partial class Location
{
    public int Id { get; set; }

    public string? LocationName { get; set; }

    [JsonIgnore]
    public virtual ICollection<Tour> TourEndLocations { get; set; } = new List<Tour>();

    [JsonIgnore]
    public virtual ICollection<Tour> TourStartLocations { get; set; } = new List<Tour>();
}
