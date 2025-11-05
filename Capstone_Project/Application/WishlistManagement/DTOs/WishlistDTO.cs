namespace BE_Capstone_Project.Application.WishlistManagement.DTOs
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
        public string Username { get; set; } = string.Empty;
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
