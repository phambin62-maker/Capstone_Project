using System;
using System.Collections.Generic;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.Infrastructure;

public partial class OtmsdbContext : DbContext
{
    public OtmsdbContext()
    {
    }

    public OtmsdbContext(DbContextOptions<OtmsdbContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(" Connection string 'DefaultConnection' not found!");

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    public virtual DbSet<Company> Companies { get; set; }
    public virtual DbSet<Feature> Features { get; set; }
    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingCustomer> BookingCustomers { get; set; }

    public virtual DbSet<CancelCondition> CancelConditions { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourCategory> TourCategories { get; set; }

    public virtual DbSet<TourImage> TourImages { get; set; }

    public virtual DbSet<TourPriceHistory> TourPriceHistories { get; set; }

    public virtual DbSet<TourSchedule> TourSchedules { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3214EC27666AD904");

            entity.ToTable("Booking");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookingDate).HasColumnType("datetime");
            entity.Property(e => e.CertificateId)
                .HasMaxLength(50)
                .HasColumnName("CertificateID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExpirationTime).HasColumnType("smalldatetime");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TourScheduleId).HasColumnName("TourScheduleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.TourSchedule).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TourScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__TourSch__6477ECF3");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__UserID__6383C8BA");
        });

        modelBuilder.Entity<BookingCustomer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingC__3214EC271E35F1B3");

            entity.ToTable("BookingCustomer");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CustomerType).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.IdentityId)
                .HasMaxLength(50)
                .HasColumnName("IdentityID");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingCustomers)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingCu__Booki__656C112C");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyID).HasName("PK__Company__4D913760");

            entity.ToTable("Company");

            entity.Property(e => e.CompanyID).HasColumnName("CompanyID");
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.TaxCode).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Description);
            entity.Property(e => e.LogoUrl).HasMaxLength(300);
            entity.Property(e => e.FoundedYear);
            
            // About Us Section Fields
            entity.Property(e => e.AboutUsTitle).HasMaxLength(200);
            entity.Property(e => e.AboutUsDescription1);
            entity.Property(e => e.AboutUsDescription2);
            entity.Property(e => e.AboutUsImageUrl).HasMaxLength(300);
            entity.Property(e => e.AboutUsImageAlt).HasMaxLength(100);
            entity.Property(e => e.ExperienceNumber).HasMaxLength(50);
            entity.Property(e => e.ExperienceText).HasMaxLength(100);
            
            // Stats Fields
            entity.Property(e => e.HappyTravelersCount);
            entity.Property(e => e.CountriesCoveredCount);
            entity.Property(e => e.YearsExperienceCount);
            
            entity.Property(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CancelCondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CancelCo__3214EC275035C402");

            entity.ToTable("CancelCondition");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Chat__3214EC27C2840407");

            entity.ToTable("Chat");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.SentDate).HasColumnType("datetime");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.Customer).WithMany(p => p.ChatCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chat__CustomerID__5812160E");

            entity.HasOne(d => d.Staff).WithMany(p => p.ChatStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chat__StaffID__571DF1D5");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC271C940BCA");

            entity.ToTable("Location");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationName).HasMaxLength(250);
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__News__3214EC27F261B932");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.News)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__News__UserID__5535A963");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC278158AC28");

            entity.ToTable("Notification");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.Property(e => e.IsRead);
            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__5629CD9C");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Review__3214EC2742A083D7");

            entity.ToTable("Review");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.TourId).HasColumnName("TourID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__BookingI__5AEE82B9");

            entity.HasOne(d => d.Tour).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__TourID__59FA5E80");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__UserID__59063A47");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC27574568B1");

            entity.ToTable("Role");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tour__3214EC27922073D7");

            entity.ToTable("Tour");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CancelConditionId).HasColumnName("CancelConditionID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ChildDiscount).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EndLocationId).HasColumnName("EndLocationID");
            entity.Property(e => e.GroupDiscount).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StartLocationId).HasColumnName("StartLocationID");

            entity.HasOne(d => d.CancelCondition).WithMany(p => p.Tours)
                .HasForeignKey(d => d.CancelConditionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tour__CancelCond__60A75C0F");

            entity.HasOne(d => d.Category).WithMany(p => p.Tours)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tour__CategoryID__5FB337D6");

            entity.HasOne(d => d.EndLocation).WithMany(p => p.TourEndLocations)
                .HasForeignKey(d => d.EndLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tour__EndLocatio__5EBF139D");

            entity.HasOne(d => d.StartLocation).WithMany(p => p.TourStartLocations)
                .HasForeignKey(d => d.StartLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tour__StartLocat__5DCAEF64");
        });

        modelBuilder.Entity<TourCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourCate__3214EC2742F6D3C1");

            entity.ToTable("TourCategory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<TourImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourImag__3214EC275B48A654");

            entity.ToTable("TourImage");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.TourId).HasColumnName("TourID");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourImages)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourImage__TourI__619B8048");
        });

        modelBuilder.Entity<TourPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourPric__3214EC27E8D812A6");

            entity.ToTable("TourPriceHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChildrenDiscount).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.GroupDiscount).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TourId).HasColumnName("TourID");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourPriceHistories)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourPrice__TourI__66603565");
        });

        modelBuilder.Entity<TourSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourSche__3214EC27D9098D1C");

            entity.ToTable("TourSchedule");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TourId).HasColumnName("TourID");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourSchedules)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourSched__TourI__628FA481");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC274AAAED85");

            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__RoleID__5441852A");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Wishlist__3214EC2741A85014");

            entity.ToTable("Wishlist");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TourId).HasColumnName("TourID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Tour).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wishlist__TourID__5CD6CB2B");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wishlist__UserID__5BE2A6F2");
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feature__3214EC27");

            entity.ToTable("Feature");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Delay);
            entity.Property(e => e.DisplayOrder);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
