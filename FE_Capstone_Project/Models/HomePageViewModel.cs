namespace FE_Capstone_Project.Models
{
    public class HomePageViewModel
    {
        public List<TourRatingViewModel> TourRatings { get; set; } = new();
        public List<ReviewViewModel> Reviews { get; set; } = new();
    }

}
