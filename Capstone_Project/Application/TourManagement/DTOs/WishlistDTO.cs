namespace BE_Capstone_Project.Application.TourManagement.DTOs
{
    public class WishlistDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public DateTime AddedDate { get; set; }
    }

    public class AddWishlistRequest
    {
        public int TourId { get; set; }
    }

    public class WishlistResponse
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; }
        public decimal? TourPrice { get; set; }
        public byte Duration { get; set; }
        public string TourImage { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
