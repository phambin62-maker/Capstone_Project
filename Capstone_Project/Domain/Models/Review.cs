using BE_Capstone_Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Domain.Models;

public partial class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TourId { get; set; }

    public int BookingId { get; set; }

    public byte? Stars { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? ReviewStatus { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    [JsonIgnore]
    public virtual Tour Tour { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
