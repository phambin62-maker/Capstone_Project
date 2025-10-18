using BE_Capstone_Project.Domain.Enums;
using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class BookingCustomer
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? IdentityId { get; set; }

    public CustomerType? CustomerType { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
