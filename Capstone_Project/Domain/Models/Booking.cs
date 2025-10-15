using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TourScheduleId { get; set; }

    public byte? PaymentStatus { get; set; }

    public DateOnly? RefundDate { get; set; }

    public decimal? RefundAmount { get; set; }

    public byte? PaymentMethod { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public DateTime? ExpirationTime { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? CertificateId { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? BookingDate { get; set; }

    public byte? BookingStatus { get; set; }

    public virtual ICollection<BookingCustomer> BookingCustomers { get; set; } = new List<BookingCustomer>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual TourSchedule TourSchedule { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
