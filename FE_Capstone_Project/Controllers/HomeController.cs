using BE_Capstone_Project.Application.ReviewManagement.DTOs;
using BE_Capstone_Project.Application.Company.DTOs;
using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

namespace FE_Capstone_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiHelper _apiHelper;
        public HomeController(ILogger<HomeController> logger, ApiHelper apiHelper)
        {
            _logger = logger;
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomePageViewModel();
            var firstName = HttpContext.Session.GetString("FirstName");
            ViewBag.FirstName = firstName;

            try
            {
                var reviewResponse = await _apiHelper.GetAsync<List<ReviewViewModel>>("Review/get-all");
                model.Reviews = reviewResponse ?? new List<ReviewViewModel>();

                var ratingResponse = await _apiHelper.GetAsync<List<TourRatingViewModel>>("Review/tour-ratings");
                model.TourRatings = ratingResponse ?? new List<TourRatingViewModel>();

                // --- Load Destinations và Categories cho form search ---
                var destinationsResponse = await _apiHelper.GetAsync<LocationsResponse>("Locations/GetAllLocations");
                var categoriesResponse = await _apiHelper.GetAsync<TourCategoriesResponse>("TourCategories/GetAllTourCategories");

                ViewBag.Destinations = destinationsResponse?.Data ?? new List<LocationViewModel>();
                ViewBag.Categories = categoriesResponse?.Data ?? new List<TourCategoryViewModel>();

                // --- Load Features cho Why Choose Us section từ API ---
                var featuresResponse = await _apiHelper.GetAsync<List<FeatureDTO>>("Feature/active");
                if (featuresResponse != null && featuresResponse.Any())
                {
                    model.Features = featuresResponse.Select(f => new FeatureViewModel
                    {
                        Icon = f.Icon,
                        Title = f.Title,
                        Description = f.Description ?? string.Empty,
                        Delay = f.Delay
                    }).ToList();
                }
                else
                {
                    // Fallback nếu không có dữ liệu từ API
                    model.Features = new List<FeatureViewModel>
                    {
                        new FeatureViewModel
                        {
                            Icon = "bi bi-people-fill",
                            Title = "Local Experts",
                            Description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium totam.",
                            Delay = 200
                        },
                        new FeatureViewModel
                        {
                            Icon = "bi bi-shield-check",
                            Title = "Safe & Secure",
                            Description = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum.",
                            Delay = 250
                        },
                        new FeatureViewModel
                        {
                            Icon = "bi bi-cash",
                            Title = "Best Prices",
                            Description = "Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet consectetur adipisci velit.",
                            Delay = 300
                        },
                        new FeatureViewModel
                        {
                            Icon = "bi bi-headset",
                            Title = "24/7 Support",
                            Description = "Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam nisi.",
                            Delay = 350
                        },
                        new FeatureViewModel
                        {
                            Icon = "bi bi-geo-alt-fill",
                            Title = "Global Destinations",
                            Description = "Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae.",
                            Delay = 400
                        },
                        new FeatureViewModel
                        {
                            Icon = "bi bi-star-fill",
                            Title = "Premium Experience",
                            Description = "Excepteur sint occaecat cupidatat non proident sunt in culpa qui officia deserunt mollit anim.",
                            Delay = 450
                        }
                    };
                }

                // --- Load Featured Tours for the homepage ---
                var toursResponse = await _apiHelper.GetAsync<TourListResponse>("Tour/GetPaginatedTours?page=1&pageSize=6");
                if (toursResponse?.Tours != null && toursResponse.Tours.Any())
                {
                    model.FeaturedTours = toursResponse.Tours
                        .Where(t => t.TourStatus)
                        .Take(6)
                        .ToList();
                }

                // --- Load Latest News ---
                var newsResponse = await _apiHelper.GetAsync<List<NewsViewModel>>("News");
                if (newsResponse != null && newsResponse.Any())
                {
                    model.LatestNews = newsResponse
                        .Where(n => n.NewsStatus == "1" || string.Equals(n.NewsStatus, "published", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(n => n.CreatedDate ?? DateTime.MinValue)
                        .Take(3)
                        .ToList();
                }

                // --- Load About Us content từ Company API ---
                var companyResponse = await _apiHelper.GetAsync<CompanyDTO>("Company/active");
                if (companyResponse != null)
                {
                    model.AboutUs = new AboutUsViewModel
                    {
                        Title = companyResponse.AboutUsTitle ?? "Explore the World with Confidence",
                        Description1 = companyResponse.AboutUsDescription1 ?? string.Empty,
                        Description2 = companyResponse.AboutUsDescription2 ?? string.Empty,
                        ImageUrl = companyResponse.AboutUsImageUrl ?? "~/assets/img/travel/showcase-8.webp",
                        ImageAlt = companyResponse.AboutUsImageAlt ?? "Travel Experience",
                        ExperienceNumber = companyResponse.ExperienceNumber ?? "15+",
                        ExperienceText = companyResponse.ExperienceText ?? "Years of Excellence",
                        Stats = new List<StatViewModel>
                        {
                            new StatViewModel
                            {
                                StartValue = 0,
                                EndValue = companyResponse.HappyTravelersCount ?? 1200,
                                Duration = 2,
                                Label = "Happy Travelers"
                            },
                            new StatViewModel
                            {
                                StartValue = 0,
                                EndValue = companyResponse.CountriesCoveredCount ?? 85,
                                Duration = 2,
                                Label = "Countries Covered"
                            },
                            new StatViewModel
                            {
                                StartValue = 0,
                                EndValue = companyResponse.YearsExperienceCount ?? 15,
                                Duration = 2,
                                Label = "Years Experience"
                            }
                        }
                    };
                }
                else
                {
                    // Fallback nếu không có dữ liệu từ API
                    model.AboutUs = new AboutUsViewModel
                    {
                        Title = "Explore the World with Confidence",
                        Description1 = "Wanderlust Horizons Co., Ltd. Travel is proud to be a leading travel company dedicated to providing safe, comfortable, and memorable journeys for every customer. With a team of professional tour guides and a global network of partners, we are committed to delivering the highest quality service in every trip you take.",
                        Description2 = "From unique cultural discovery tours to luxurious relaxation experiences, FastRail Travel accompanies you on every journey. We believe that each trip is not only about exploring the world but also about enjoying life, making meaningful connections, and creating unforgettable memories.",
                        ImageUrl = "assets/img/travel/showcase-8.webp",
                        ImageAlt = "Travel Experience",
                        ExperienceNumber = "15+",
                        ExperienceText = "Years of Excellence",
                        Stats = new List<StatViewModel>
                        {
                            new StatViewModel
                            {
                                StartValue = 0,
                                EndValue = 1200,
                                Duration = 2,
                                Label = "Happy Travelers"
                            },
                            new StatViewModel
                            {
                                StartValue = 0,
                                EndValue = 85,
                                Duration = 2,
                                Label = "Countries Covered"
                            },
                            new StatViewModel
                            {
                                StartValue = 0,
                                EndValue = 15,
                                Duration = 2,
                                Label = "Years Experience"
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error fetching data: {ex.Message}";
                ViewBag.Destinations = new List<LocationViewModel>();
                ViewBag.Categories = new List<TourCategoryViewModel>();
                model.Features = new List<FeatureViewModel>();
                model.AboutUs = new AboutUsViewModel();
                model.FeaturedTours = new List<TourViewModel>();
                model.LatestNews = new List<NewsViewModel>();
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Forbidden()
        {
            ViewData["Title"] = "Không có quyền truy cập";
            return View();
        }
    }
}
