using BE_Capstone_Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Domain.Models;

public partial class Tour
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Duration { get; set; }

    public int StartLocationId { get; set; }

    public int EndLocationId { get; set; }

    public int CategoryId { get; set; }

    public int CancelConditionId { get; set; }

    public decimal? ChildDiscount { get; set; }

    public decimal? GroupDiscount { get; set; }

    public byte? GroupNumber { get; set; }

    public bool? TourStatus { get; set; }

    public short? MinSeats { get; set; }

    public short? MaxSeats { get; set; }

    public virtual CancelCondition CancelCondition { get; set; } = null!;

    public virtual TourCategory Category { get; set; } = null!;

    public virtual Location EndLocation { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Location StartLocation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<TourImage> TourImages { get; set; } = new List<TourImage>();

    public virtual ICollection<TourPriceHistory> TourPriceHistories { get; set; } = new List<TourPriceHistory>();

    public virtual ICollection<TourSchedule> TourSchedules { get; set; } = new List<TourSchedule>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
