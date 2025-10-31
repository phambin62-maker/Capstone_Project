using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Domain.Models;

public partial class TourCategory
{
    public int Id { get; set; }

    public string? CategoryName { get; set; }

    [JsonIgnore]
    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
