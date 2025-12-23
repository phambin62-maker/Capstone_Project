namespace FE_Capstone_Project.Models
{
    public class HomePageViewModel
    {
        public List<TourRatingViewModel> TourRatings { get; set; } = new();
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public List<FeatureViewModel> Features { get; set; } = new();
        public AboutUsViewModel AboutUs { get; set; } = new();
        public List<TourViewModel> FeaturedTours { get; set; } = new();
        public List<NewsViewModel> LatestNews { get; set; } = new();
    }

    public class FeatureViewModel
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Delay { get; set; } = 200; // AOS delay in milliseconds
    }

    public class AboutUsViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description1 { get; set; } = string.Empty;
        public string Description2 { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageAlt { get; set; } = string.Empty;
        public string ExperienceNumber { get; set; } = string.Empty;
        public string ExperienceText { get; set; } = string.Empty;
        public List<StatViewModel> Stats { get; set; } = new();
    }

    public class StatViewModel
    {
        public int StartValue { get; set; } = 0;
        public int EndValue { get; set; } = 0;
        public int Duration { get; set; } = 2;
        public string Label { get; set; } = string.Empty;
    }
}
