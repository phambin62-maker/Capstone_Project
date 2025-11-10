using System.Text.Json.Serialization;

namespace BE_Capstone_Project.Application.Company.DTOs
{
    public class CompanyDTO
    {
        [JsonPropertyName("companyID")]
        public int CompanyID { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonPropertyName("licenseNumber")]
        public string? LicenseNumber { get; set; }

        [JsonPropertyName("taxCode")]
        public string? TaxCode { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("logoUrl")]
        public string? LogoUrl { get; set; }

        [JsonPropertyName("foundedYear")]
        public int? FoundedYear { get; set; }
        
        // About Us Section
        [JsonPropertyName("aboutUsTitle")]
        public string? AboutUsTitle { get; set; }

        [JsonPropertyName("aboutUsDescription1")]
        public string? AboutUsDescription1 { get; set; }

        [JsonPropertyName("aboutUsDescription2")]
        public string? AboutUsDescription2 { get; set; }

        [JsonPropertyName("aboutUsImageUrl")]
        public string? AboutUsImageUrl { get; set; }

        [JsonPropertyName("aboutUsImageAlt")]
        public string? AboutUsImageAlt { get; set; }

        [JsonPropertyName("experienceNumber")]
        public string? ExperienceNumber { get; set; }

        [JsonPropertyName("experienceText")]
        public string? ExperienceText { get; set; }
        
        // Stats
        [JsonPropertyName("happyTravelersCount")]
        public int? HappyTravelersCount { get; set; }

        [JsonPropertyName("countriesCoveredCount")]
        public int? CountriesCoveredCount { get; set; }

        [JsonPropertyName("yearsExperienceCount")]
        public int? YearsExperienceCount { get; set; }
        
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}

