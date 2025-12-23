using BE_Capstone_Project.Domain.Enums;
using System;
using System.Collections.Generic;

namespace BE_Capstone_Project.Domain.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PasswordHash { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Image { get; set; }

    public int RoleId { get; set; }
    public string? Provider { get; set; }

    public UserStatus? UserStatus { get; set; }
    public string? PasswordResetTokenHash { get; set; } // hash của token
    public DateTime? PasswordResetExpires { get; set; }

    public DateTime? CreatedDate { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Chat> ChatCustomers { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatStaffs { get; set; } = new List<Chat>();

    public virtual ICollection<News> News { get; set; } = new List<News>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
