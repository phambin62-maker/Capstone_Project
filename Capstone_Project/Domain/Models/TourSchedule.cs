using BE_Capstone_Project.Domain.Enums;
using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class TourSchedule
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public DateOnly? DepartureDate { get; set; }

    public DateOnly? ArrivalDate { get; set; }

    public ScheduleStatus? ScheduleStatus { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Tour Tour { get; set; } = null!;
}
