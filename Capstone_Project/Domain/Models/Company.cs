using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_Capstone_Project.Domain.Models
{
    [Table("Company")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyID { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        [StringLength(50)]
        public string? TaxCode { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [Url]
        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public string? Description { get; set; }

        [StringLength(300)]
        public string? LogoUrl { get; set; }

        public int? FoundedYear { get; set; }

        // About Us Section Fields
        [StringLength(200)]
        public string? AboutUsTitle { get; set; }

        public string? AboutUsDescription1 { get; set; }

        public string? AboutUsDescription2 { get; set; }

        [StringLength(300)]
        public string? AboutUsImageUrl { get; set; }

        [StringLength(100)]
        public string? AboutUsImageAlt { get; set; }

        [StringLength(50)]
        public string? ExperienceNumber { get; set; }

        [StringLength(100)]
        public string? ExperienceText { get; set; }

        // Stats Fields
        public int? HappyTravelersCount { get; set; }

        public int? CountriesCoveredCount { get; set; }

        public int? YearsExperienceCount { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
