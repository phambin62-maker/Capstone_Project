using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class CancelCondition
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public byte? MinDaysBeforeTrip { get; set; }

    public byte? RefundPercent { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? CancelStatus { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
