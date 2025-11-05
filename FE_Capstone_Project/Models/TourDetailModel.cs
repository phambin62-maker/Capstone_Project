//using System.Text.Json.Serialization;

//namespace FE_Capstone_Project.Models
//{
//    public class TourDetailModel
//    {
//        public int Id { get; set; }
//        public string Name { get; set; } = string.Empty;
//        public string Description { get; set; } = string.Empty;
//        public decimal Price { get; set; }
//        public byte Duration { get; set; }

//        public int StartLocationId { get; set; }
//        public string StartLocationName { get; set; } = string.Empty;

//        public int EndLocationId { get; set; }
//        public string EndLocationName { get; set; } = string.Empty;

//        public int CategoryId { get; set; }
//        public string CategoryName { get; set; } = string.Empty;

//        public int CancelConditionId { get; set; }
//        public string CancelConditionName { get; set; } = string.Empty;

//        public decimal? ChildDiscount { get; set; }
//        public decimal? GroupDiscount { get; set; }
//        public byte? GroupNumber { get; set; }
//        public short? MinSeats { get; set; }
//        public short? MaxSeats { get; set; }
//        public bool TourStatus { get; set; }
//        public List<string> Images { get; set; } = new List<string>();
//    }
//    public class LocationResponse
//    {
//        [JsonPropertyName("id")]
//        public int Id { get; set; }

//        [JsonPropertyName("name")]
//        public string Name { get; set; } = string.Empty;
//    }

//    public class CategoryResponse
//    {
//        [JsonPropertyName("id")]
//        public int Id { get; set; }

//        [JsonPropertyName("name")]
//        public string Name { get; set; } = string.Empty;
//    }

//    public class CancelConditionResponse
//    {
//        [JsonPropertyName("id")]
//        public int Id { get; set; }

//        [JsonPropertyName("name")]
//        public string Name { get; set; } = string.Empty;
//    }
//}