using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    public class WishlistModels
    {
        public class WishlistData
        {
            public int Id { get; set; }
            public int TourId { get; set; }
            public string TourName { get; set; }
            public decimal? TourPrice { get; set; }
            public byte Duration { get; set; }
            public string TourImage { get; set; }
            public DateTime AddedDate { get; set; }
        }

        public class WishlistResponseModel
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
            [JsonPropertyName("wishlist")]
            public List<WishlistData> Wishlist { get; set; }
        }
    }
}
