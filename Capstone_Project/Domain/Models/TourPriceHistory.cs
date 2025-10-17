using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class TourPriceHistory
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public decimal? Price { get; set; }

    public decimal? ChildrenDiscount { get; set; }

    public decimal? GroupDiscount { get; set; }

    public int? GroupNumber { get; set; }

    public DateOnly? StartPriceDate { get; set; }

    public DateOnly? EndPriceDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Tour Tour { get; set; } = null!;
}
