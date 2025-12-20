using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Models;
using FE_Capstone_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FE_Capstone_Project.Controllers
{
    [AuthorizeRole(2)]
    public class StaffController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffController> _logger;
        private const string BASE_API_URL = "https://localhost:7160/api/";
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DataService _dataService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StaffController(IHttpClientFactory httpClientFactory, ILogger<StaffController> logger, DataService dataService, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BASE_API_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            _dataService = dataService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");
        }

        private int GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue && userId.Value != 0)
            {
                return userId.Value;
            }

            _logger.LogWarning("Could not find 'UserId' in Session.");
            return 0;
        }

        private async Task<(bool Success, T Data, string Error)> CallApiAsync<T>(
            string endpoint,
            HttpMethod method = null,
            HttpContent content = null)
        {
            try
            {
                var request = new HttpRequestMessage(method ?? HttpMethod.Get, endpoint);
                if (content != null)
                    request.Content = content;

                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning($"[StaffController] No token found for request to {endpoint}");
                    return (false, default, "Token not found. Please log in again.");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"API Call: {endpoint}, Status: {response.StatusCode}, Response: {responseContent}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning($"Unauthorized access to {endpoint}");
                    return (false, default, "Login session has expired. Please log in again.");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning($"Forbidden access to {endpoint}. User lacks required permissions.");
                    return (false, default, "You don't have permission to perform this action. Please contact administrator.");
                }

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    JsonElement root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataElement))
                    {
                        var result = JsonSerializer.Deserialize<T>(dataElement.GetRawText(), _jsonOptions);
                        return (true, result, string.Empty);
                    }
                    else
                    {
                        var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                        return (true, result, string.Empty);
                    }
                }
                else
                {
                    return (false, default, $"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API call exception: {Endpoint}", endpoint);
                return (false, default, $"Exception: {ex.Message}");
            }
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Staff Dashboard";
            return View();
        }
        public IActionResult Chat()
        {
            ViewData["Title"] = "Staff Dashboard";
            return View();
        }

        public async Task<List<TourViewModel>> LoadToursWithDetails(List<TourViewModel> tours)
        {
            var detailedTours = new List<TourViewModel>();

            foreach (var tour in tours)
            {
                var detailedTour = await LoadTourDetails(tour);
                detailedTours.Add(detailedTour);
            }

            return detailedTours;
        }

        public async Task<TourViewModel> LoadTourDetails(TourViewModel tour)
        {
            if (tour.CategoryId > 0)
            {
                var (catSuccess, category, _) = await CallApiAsync<TourCategory>($"TourCategories/{tour.CategoryId}");
                if (catSuccess && category != null)
                {
                    tour.Category = category;
                }
            }

            if (tour.StartLocationId > 0)
            {
                var (startLocSuccess, startLocation, _) = await CallApiAsync<Location>($"Locations/{tour.StartLocationId}");
                if (startLocSuccess && startLocation != null)
                {
                    tour.StartLocation = startLocation;
                }
            }

            if (tour.EndLocationId > 0)
            {
                var (endLocSuccess, endLocation, _) = await CallApiAsync<Location>($"Locations/{tour.EndLocationId}");
                if (endLocSuccess && endLocation != null)
                {
                    tour.EndLocation = endLocation;
                }
            }

            return tour;
        }

        public async Task<IActionResult> Tours(
            int page = 1,
            int pageSize = 5,
            string status = null,
            int? startLocation = null,
            int? endLocation = null,
            int? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sort = null,
            string search = null)
        {
            ViewData["Title"] = "Tour Management";

            try
            {
                bool? statusFilter = null;
                if (!string.IsNullOrEmpty(status))
                {
                    if (status.ToLower() == "active")
                        statusFilter = true;
                    else if (status.ToLower() == "inactive")
                        statusFilter = false;
                }

                var apiUrl = $"Tour/GetFilteredTours?page={page}&pageSize={pageSize}";

                if (statusFilter.HasValue)
                    apiUrl += $"&status={statusFilter.Value}";
                if (startLocation.HasValue)
                    apiUrl += $"&startLocation={startLocation.Value}";
                if (endLocation.HasValue)
                    apiUrl += $"&endLocation={endLocation.Value}";
                if (category.HasValue)
                    apiUrl += $"&category={category.Value}";
                if (minPrice.HasValue)
                    apiUrl += $"&minPrice={minPrice.Value}";
                if (maxPrice.HasValue)
                    apiUrl += $"&maxPrice={maxPrice.Value}";
                if (!string.IsNullOrEmpty(sort))
                    apiUrl += $"&sort={sort}";
                if (!string.IsNullOrEmpty(search))
                    apiUrl += $"&search={search}";

                var (success, toursResponse, error) = await CallApiAsync<TourListResponse>(apiUrl);

                if (!success)
                {
                    ViewBag.ErrorMessage = $"Unable to load tour list: {error}";
                    return View(new List<TourViewModel>());
                }

                var tours = toursResponse?.Tours ?? new List<TourViewModel>();
                tours = await LoadToursWithDetails(tours);

                var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
                var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");

                var countApiUrl = "Tour/GetFilteredTourCount";
                if (statusFilter.HasValue)
                    countApiUrl += $"?status={statusFilter.Value}";
                if (startLocation.HasValue)
                    countApiUrl += $"&startLocation={startLocation.Value}";
                if (endLocation.HasValue)
                    countApiUrl += $"&endLocation={endLocation.Value}";
                if (category.HasValue)
                    countApiUrl += $"&category={category.Value}";
                if (minPrice.HasValue)
                    countApiUrl += $"&minPrice={minPrice.Value}";
                if (maxPrice.HasValue)
                    countApiUrl += $"&maxPrice={maxPrice.Value}";
                if (!string.IsNullOrEmpty(search))
                    countApiUrl += $"&search={search}";

                var (countSuccess, countResponse, countError) = await CallApiAsync<TourCountResponse>(countApiUrl);
                var totalCount = countSuccess ? countResponse?.TourCount ?? tours.Count : tours.Count;

                ViewBag.Locations = locSuccess ? locations : new List<Location>();
                ViewBag.Categories = catSuccess ? categories : new List<TourCategory>();
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                ViewBag.CurrentStatus = status;
                ViewBag.CurrentStartLocation = startLocation;
                ViewBag.CurrentEndLocation = endLocation;
                ViewBag.CurrentCategory = category;
                ViewBag.CurrentMinPrice = minPrice;
                ViewBag.CurrentMaxPrice = maxPrice;
                ViewBag.CurrentSort = sort;
                ViewBag.CurrentSearch = search;

                var totalTours = totalCount;
                var activeTours = tours.Count(t => t.TourStatus);
                var inactiveTours = tours.Count(t => !t.TourStatus);
                var displayingTours = tours.Count;

                ViewBag.TotalTours = totalTours;
                ViewBag.ActiveTours = activeTours;
                ViewBag.InactiveTours = inactiveTours;
                ViewBag.DisplayingTours = displayingTours;

                return View(tours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tours");
                ViewBag.ErrorMessage = "Server connection error. Please check your connection.";

                ViewBag.TotalTours = 0;
                ViewBag.ActiveTours = 0;
                ViewBag.InactiveTours = 0;
                ViewBag.DisplayingTours = 0;

                return View(new List<TourViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTourDetailImage(int tourId, int? imageIndex = null)
        {
            try
            {
                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{tourId}");

                if (!success || result?.Tour == null || result.Tour.TourImages == null || !result.Tour.TourImages.Any())
                {
                    return File(System.IO.File.ReadAllBytes("wwwroot/images/default-tour.jpg"), "image/jpeg");
                }

                TourImage image;
                if (imageIndex.HasValue && imageIndex.Value < result.Tour.TourImages.Count)
                {
                    image = result.Tour.TourImages[imageIndex.Value];
                }
                else
                {
                    image = result.Tour.TourImages.First();
                }

                if (image.Image.StartsWith("data:image"))
                {
                    var base64Data = image.Image.Split(',')[1];
                    var imageBytes = Convert.FromBase64String(base64Data);
                    return File(imageBytes, "image/jpeg");
                }
                else if (!string.IsNullOrEmpty(image.Image))
                {
                    var imageResponse = await _httpClient.GetAsync($"Tour/GetImage?path={Uri.EscapeDataString(image.Image)}");
                    if (imageResponse.IsSuccessStatusCode)
                    {
                        var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                        var contentType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                        return File(imageBytes, contentType);
                    }
                }

                return File(System.IO.File.ReadAllBytes("wwwroot/images/default-tour.jpg"), "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour detail image for ID: {TourId}", tourId);
                return File(System.IO.File.ReadAllBytes("wwwroot/images/default-tour.jpg"), "image/jpeg");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTourImageGallery(int tourId)
        {
            try
            {
                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{tourId}");

                if (!success || result?.Tour == null || result.Tour.TourImages == null || !result.Tour.TourImages.Any())
                {
                    return Json(new { success = false, images = new List<object>() });
                }

                var images = result.Tour.TourImages.Select((img, index) => new
                {
                    index = index,
                    url = Url.Action("GetTourDetailImage", "Staff", new { tourId = tourId, imageIndex = index }),
                    isFeatured = index == 0
                }).ToList<object>();

                return Json(new { success = true, images = images });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour image gallery for ID: {TourId}", tourId);
                return Json(new { success = false, images = new List<object>() });
            }
        }

        public async Task<IActionResult> TourDetails(int id)
        {
            try
            {
                _logger.LogInformation($"Loading tour details for ID: {id}");

                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{id}");

                if (!success || result?.Tour == null)
                {
                    TempData["ErrorMessage"] = $"Tour with ID {id} not found. Error: {error}";
                    return RedirectToAction("Tours");
                }

                var tourDetail = new TourDetailModel
                {
                    Id = result.Tour.Id,
                    Name = result.Tour.Name,
                    Description = result.Tour.Description,
                    Price = result.Tour.Price,
                    Duration = result.Tour.Duration,
                    StartLocationId = result.Tour.StartLocationId,
                    EndLocationId = result.Tour.EndLocationId,
                    CategoryId = result.Tour.CategoryId,
                    CancelConditionId = result.Tour.CancelConditionId,
                    ChildDiscount = result.Tour.ChildDiscount,
                    GroupDiscount = result.Tour.GroupDiscount,
                    GroupNumber = result.Tour.GroupNumber,
                    MinSeats = result.Tour.MinSeats,
                    MaxSeats = result.Tour.MaxSeats,
                    TourStatus = result.Tour.TourStatus,
                    TourImages = result.Tour.TourImages ?? new List<TourImage>(),
                    Reviews = result.Tour.Reviews ?? new List<Review>()
                };

                try
                {
                    await LoadAdditionalInfo(tourDetail);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load additional info for tour {TourId}", id);
                }

                ViewData["Title"] = $"Tour Details - {tourDetail.Name}";
                return View(tourDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tour details for ID: {TourId}", id);
                TempData["ErrorMessage"] = $"Server connection error: {ex.Message}";
                return RedirectToAction("Tours");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTourEditImage(int tourId, int? imageIndex = null)
        {
            try
            {
                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{tourId}");

                if (!success || result?.Tour == null || result.Tour.TourImages == null || !result.Tour.TourImages.Any())
                {
                    return await GetDefaultImage();
                }

                TourImage image;
                if (imageIndex.HasValue && imageIndex.Value < result.Tour.TourImages.Count)
                {
                    image = result.Tour.TourImages[imageIndex.Value];
                }
                else
                {
                    image = result.Tour.TourImages.First();
                }

                if (image.Image.StartsWith("data:image"))
                {
                    var base64Data = image.Image.Split(',')[1];
                    var imageBytes = Convert.FromBase64String(base64Data);
                    return File(imageBytes, "image/jpeg");
                }
                else if (!string.IsNullOrEmpty(image.Image))
                {
                    var imageResponse = await _httpClient.GetAsync($"Tour/GetImage?path={Uri.EscapeDataString(image.Image)}");
                    if (imageResponse.IsSuccessStatusCode)
                    {
                        var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                        var contentType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                        return File(imageBytes, contentType);
                    }
                }

                return await GetDefaultImage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour edit image for ID: {TourId}", tourId);
                return await GetDefaultImage();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTourImage(int imageId)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                var request = new HttpRequestMessage(HttpMethod.Post, $"Tour/DeleteTourImage?imageId={imageId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Image deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete image" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tour image ID: {ImageId}", imageId);
                return Json(new { success = false, message = "Error deleting image" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDefaultImage()
        {
            try
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-tour.jpg");
                if (System.IO.File.Exists(imagePath))
                {
                    return File(System.IO.File.ReadAllBytes(imagePath), "image/jpeg");
                }
                else
                {
                    var placeholderSvg = "<svg width='400' height='300' xmlns='http://www.w3.org/2000/svg'><rect width='400' height='300' fill='#f8f9fa'/><text x='200' y='150' text-anchor='middle' font-family='Arial' font-size='16' fill='#6c757d'>No Image</text></svg>";
                    var bytes = Encoding.UTF8.GetBytes(placeholderSvg);
                    return File(bytes, "image/svg+xml");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default image");
                return NotFound();
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation($"Loading tour for edit ID: {id}");

                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{id}");
                if (!success || result?.Tour == null)
                {
                    TempData["ErrorMessage"] = $"Tour with ID {id} not found. Error: {error}";
                    return RedirectToAction("Tours");
                }

                _logger.LogInformation($"Tour {id} has {result.Tour.TourImages?.Count ?? 0} images");
                if (result.Tour.TourImages != null)
                {
                    foreach (var img in result.Tour.TourImages)
                    {
                        _logger.LogInformation($"Image ID: {img?.Id}, Path: {img?.Image ?? "null"}");
                    }
                }

                var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
                var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");
                var (cancelSuccess, cancelConditions, _) = await CallApiAsync<List<CancelCondition>>("CancelCondition");

                ViewBag.Locations = locSuccess ? locations : new List<Location>();
                ViewBag.Categories = catSuccess ? categories : new List<TourCategory>();
                ViewBag.CancelConditions = cancelSuccess ? cancelConditions : new List<CancelCondition>();

                var currentTourImages = result.Tour.TourImages?
                    .Where(img => img != null)
                    .Select(img => new TourImageViewModel
                    {
                        Id = img.Id,
                        Image = img.Image ?? string.Empty,
                        TourId = img.TourId
                    })
                    .ToList() ?? new List<TourImageViewModel>();

                ViewBag.CurrentTourImages = currentTourImages;
                ViewBag.CurrentTourImagesCount = currentTourImages.Count;

                var editModel = new TourEditModel
                {
                    Id = result.Tour.Id,
                    Name = result.Tour.Name,
                    Description = result.Tour.Description,
                    Price = result.Tour.Price,
                    Duration = result.Tour.Duration,
                    StartLocationId = result.Tour.StartLocationId,
                    EndLocationId = result.Tour.EndLocationId,
                    CategoryId = result.Tour.CategoryId,
                    CancelConditionId = result.Tour.CancelConditionId,
                    ChildDiscount = result.Tour.ChildDiscount ?? 0,
                    GroupDiscount = result.Tour.GroupDiscount ?? 0,
                    GroupNumber = result.Tour.GroupNumber ?? 5,
                    MinSeats = result.Tour.MinSeats ?? 10,
                    MaxSeats = result.Tour.MaxSeats ?? 30
                };

                ViewData["Title"] = $"Edit Tour - {editModel.Name}";
                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tour for edit ID: {TourId}", id);
                TempData["ErrorMessage"] = "Server connection error.";
                return RedirectToAction("Tours");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TourEditModel model)
        {
            _logger.LogInformation($"Updating tour ID: {model.Id}");

            // Fetch current tour to get existing images count and for view reloading
            var (success, result, tourError) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{model.Id}");
            var existingImagesCount = 0;
            List<TourImageViewModel> currentImagesList = new List<TourImageViewModel>();
            if (success && result?.Tour != null && result.Tour.TourImages != null)
            {
                existingImagesCount = result.Tour.TourImages.Count;
                currentImagesList = result.Tour.TourImages.Select(img => new TourImageViewModel
                {
                    Id = img.Id,
                    Image = img.Image ?? string.Empty,
                    TourId = img.TourId
                }).ToList();
            }

            var newImagesCount = 0;
            if (model is TourEditModel editModel && editModel.Images != null)
            {
                newImagesCount = editModel.Images.Count;
            }
            else if (model.Images != null)
            {
                newImagesCount = model.Images.Count;
            }

            if (newImagesCount == 0 && existingImagesCount == 0)
            {
                ModelState.AddModelError("Images", "Tour must have at least 1 image");
            }
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }

                await ReloadViewBagData();
                ViewBag.CurrentTourImages = currentImagesList;
                ViewBag.CurrentTourImagesCount = currentImagesList.Count;
                return View(model);
            }

            try
            {
                var formData = CreateTourFormData(model);

                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    await ReloadViewBagData();
                    return View(model);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "Tour/UpdateTour");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = formData;
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"UpdateTour Response: {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // Check if anything actually changed to decide whether to show success message
                    bool hasChanged = false;
                    if (result?.Tour != null)
                    {
                        var t = result.Tour;
                        hasChanged = model.Name != t.Name ||
                                     model.Description != t.Description ||
                                     model.Price != t.Price ||
                                     model.Duration != t.Duration ||
                                     model.StartLocationId != t.StartLocationId ||
                                     model.EndLocationId != t.EndLocationId ||
                                     model.CategoryId != t.CategoryId ||
                                     model.CancelConditionId != t.CancelConditionId ||
                                     model.ChildDiscount != (t.ChildDiscount ?? 0) ||
                                     model.GroupDiscount != (t.GroupDiscount ?? 0) ||
                                     model.GroupNumber != (t.GroupNumber ?? 5) ||
                                     model.MinSeats != (t.MinSeats ?? 10) ||
                                     model.MaxSeats != (t.MaxSeats ?? 30) ||
                                     newImagesCount > 0;
                    }
                    else
                    {
                        hasChanged = true; // Fallback if we couldn't get original tour
                    }

                    if (hasChanged)
                    {
                        TempData["SuccessMessage"] = "Tour updated successfully!";
                    }

                    return RedirectToAction("Edit", new { id = model.Id });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    await ReloadViewBagData();
                    return View(model);
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "You don't have permission to update tours. Please contact administrator.";
                    await ReloadViewBagData();
                    return View(model);
                }
                else
                {
                    var errorMessage = $"Tour update failed! Status: {response.StatusCode}";
                    try
                    {
                        var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        if (errorResult.TryGetProperty("message", out var message))
                            errorMessage += $", Message: {message.GetString()}";
                    }
                    catch
                    {
                        errorMessage += $", Response: {responseContent}";
                    }

                    TempData["ErrorMessage"] = errorMessage;
                    await ReloadViewBagData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tour ID: {TourId}", model.Id);
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                await ReloadViewBagData();
                return View(model);
            }
        }

        private async Task ReloadViewBagData()
        {
            try
            {
                var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
                var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");
                var (cancelSuccess, cancelConditions, _) = await CallApiAsync<List<CancelCondition>>("CancelCondition");

                ViewBag.Locations = locSuccess ? locations : new List<Location>();
                ViewBag.Categories = catSuccess ? categories : new List<TourCategory>();
                ViewBag.CancelConditions = cancelSuccess ? cancelConditions : new List<CancelCondition>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading ViewBag data");
                ViewBag.Locations = new List<Location>();
                ViewBag.Categories = new List<TourCategory>();
                ViewBag.CancelConditions = new List<CancelCondition>();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(TourCreateModel model)
        {
            _logger.LogInformation("Creating new tour");
            if (model.Images == null || model.Images.Count == 0)
            {
                ModelState.AddModelError("Images", "Must have at least 1 image");
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for tour creation");
                await ReloadViewBagData();
                return View(model);
            }

            try
            {
                var formData = CreateTourFormData(model);

                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    await ReloadViewBagData();
                    return View(model);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "Tour/AddTour");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = formData;

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"AddTour Response: {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tour created successfully!";
                    return RedirectToAction("Tours");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    await ReloadViewBagData();
                    return View(model);
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "You don't have permission to update tours. Please contact administrator.";
                    await ReloadViewBagData();
                    return View(model);
                }
                else
                {
                    var errorMessage = $"Tour creation failed! Status: {response.StatusCode}";
                    try
                    {
                        var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        if (errorResult.TryGetProperty("message", out var message))
                            errorMessage += $", Message: {message.GetString()}";
                    }
                    catch
                    {
                        errorMessage += $", Response: {responseContent}";
                    }

                    TempData["ErrorMessage"] = errorMessage;
                    await ReloadViewBagData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tour");
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                await ReloadViewBagData();
                return View(model);
            }
        }

        public async Task LoadAdditionalInfo(TourDetailModel tourDetail)
        {
            var tasks = new List<Task>
                {
                    LoadNameFromApi($"Locations/{tourDetail.StartLocationId}", name => tourDetail.StartLocationName = name),
                    LoadNameFromApi($"Locations/{tourDetail.EndLocationId}", name => tourDetail.EndLocationName = name),
                    LoadNameFromApi($"TourCategories/{tourDetail.CategoryId}", name => tourDetail.CategoryName = name),
                    LoadNameFromApi($"CancelCondition/{tourDetail.CancelConditionId}", name => tourDetail.CancelConditionName = name)
                };

            await Task.WhenAll(tasks);
        }

        public async Task LoadNameFromApi(string endpoint, Action<string> setNameAction)
        {
            try
            {
                var (success, response, error) = await CallApiAsync<JsonElement>(endpoint);
                if (!success || response.ValueKind == JsonValueKind.Null)
                {
                    setNameAction("Not found");
                    return;
                }

                JsonElement data = response;
                if (response.TryGetProperty("data", out var dataProp))
                    data = dataProp;

                string? name = null;
                if (data.TryGetProperty("title", out var titleProp))
                    name = titleProp.GetString();
                else if (data.TryGetProperty("name", out var nameProp))
                    name = nameProp.GetString();
                else if (data.TryGetProperty("locationName", out var locProp))
                    name = locProp.GetString();
                else if (data.TryGetProperty("categoryName", out var catProp))
                    name = catProp.GetString();

                setNameAction(name ?? "No name");
            }
            catch (Exception ex)
            {
                setNameAction($"Error: {ex.Message}");
            }
        }       

        [HttpPost]
        public async Task<IActionResult> ToggleTourStatus(int id)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    return RedirectToAction("Tours");
                }

                var request = new HttpRequestMessage(HttpMethod.Post, $"Tour/ToggleTourStatus?tourId={id}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"ToggleTourStatus Response: {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TourStatusResponse>(responseContent, _jsonOptions);
                    TempData["SuccessMessage"] = result?.Message ?? "Status changed successfully!";
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Tour status change failed!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling tour status ID: {TourId}", id);
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
            }

            return RedirectToAction("Tours");
        }

        public MultipartFormDataContent CreateTourFormData(TourCreateModel model)
        {
            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(model.Name ?? ""), "Name");
            formData.Add(new StringContent(model.Description ?? ""), "Description");
            formData.Add(new StringContent(model.Price.ToString()), "Price");
            formData.Add(new StringContent(model.Duration.ToString()), "Duration");
            formData.Add(new StringContent(model.StartLocationId.ToString()), "StartLocationId");
            formData.Add(new StringContent(model.EndLocationId.ToString()), "EndLocationId");
            formData.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
            formData.Add(new StringContent(model.CancelConditionId.ToString()), "CancelConditionId");
            formData.Add(new StringContent(model.ChildDiscount.ToString()), "ChildDiscount");
            formData.Add(new StringContent(model.GroupDiscount.ToString()), "GroupDiscount");
            formData.Add(new StringContent(model.GroupNumber.ToString()), "GroupNumber");
            formData.Add(new StringContent(model.MinSeats.ToString()), "MinSeats");
            formData.Add(new StringContent(model.MaxSeats.ToString()), "MaxSeats");
            IEnumerable<IFormFile> images = model.Images;
            if (model is TourEditModel editModel)
            {
                formData.Add(new StringContent(editModel.Id.ToString()), "Id");
            }

            IEnumerable<IFormFile>? imagesToUpload = model.Images;
            if (model is TourEditModel editTourModel && editTourModel.Images != null)
            {
                imagesToUpload = editTourModel.Images;
            }

            if (imagesToUpload != null && imagesToUpload.Any())
            {
                foreach (var image in imagesToUpload.Where(img => img.Length > 0))
                {
                    var imageContent = new StreamContent(image.OpenReadStream());
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                    formData.Add(imageContent, "images", image.FileName);
                }
            }

            return formData;
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Add New Tour";

            var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
            var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");
            var (cancelSuccess, cancelConditions, _) = await CallApiAsync<List<CancelCondition>>("CancelCondition");

            ViewBag.Locations = locSuccess ? locations : new List<Location>();
            ViewBag.Categories = catSuccess ? categories : new List<TourCategory>();
            ViewBag.CancelConditions = cancelSuccess ? cancelConditions : new List<CancelCondition>();

            return View();
        }

        private string NormalizeString(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = new string(text
                .Normalize(NormalizationForm.FormD)
                .Where(c => Char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return normalized.ToLowerInvariant().Trim();
        }

        public async Task<IActionResult> News(int page = 1, int pageSize = 5, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
        {
            ViewData["Title"] = "News Management";

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Unable to identify user. Please log in again.";
                    return View(new NewsListViewModel { NewsList = new List<NewsViewModel>() });
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // ... (Phần lấy Stats giữ nguyên) ...
                var statsResponse = await _httpClient.GetAsync("News/stats");
                if (statsResponse.IsSuccessStatusCode)
                {
                    var statsContent = await statsResponse.Content.ReadAsStringAsync();
                    ViewBag.NewsStats = JsonSerializer.Deserialize<NewsStatsDTO>(statsContent, _jsonOptions);
                }
                else
                {
                    ViewBag.NewsStats = new NewsStatsDTO();
                }

                var response = await _httpClient.GetAsync($"News");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Unable to load news list. (Error: " + response.StatusCode + ")";
                    return View(new NewsListViewModel { NewsList = new List<NewsViewModel>() });
                }

                var content = await response.Content.ReadAsStringAsync();
                var newsList = JsonSerializer.Deserialize<List<NewsViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<NewsViewModel>();

                IEnumerable<NewsViewModel> filteredNews = newsList;

                // === PHẦN SỬA ĐỔI: LOGIC TÌM KIẾM CHO STAFF ===
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string normalizedSearch = NormalizeString(search);
                    filteredNews = filteredNews.Where(n =>
                    {
                        string normalizedTitle = NormalizeString(n.Title);
                        string normalizedAuthor = NormalizeString(n.AuthorName);

                        // Staff chỉ tìm theo Title hoặc Author
                        return normalizedTitle.Contains(normalizedSearch) ||
                               normalizedAuthor.Contains(normalizedSearch);
                    }).ToList();
                }
                // ===============================================

                if (fromDate.HasValue) { filteredNews = filteredNews.Where(n => n.CreatedDate?.Date >= fromDate.Value.Date); }
                if (toDate.HasValue) { filteredNews = filteredNews.Where(n => n.CreatedDate?.Date <= toDate.Value.Date); }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    string lowerStatus = status.ToLowerInvariant();
                    filteredNews = filteredNews.Where(n => n.NewsStatus != null && n.NewsStatus.ToLowerInvariant() == lowerStatus);
                }

                var sortedNews = filteredNews
                    .OrderByDescending(n => n.UpdatedDate ?? n.CreatedDate);

                var finalNewsList = sortedNews.ToList();
                int totalItems = finalNewsList.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var pagedNews = finalNewsList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var viewModel = new NewsListViewModel
                {
                    NewsList = pagedNews,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    Search = search,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Status = status
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Server connection error: " + ex.Message;
                return View(new NewsListViewModel { NewsList = new List<NewsViewModel>() });
            }
        }

        [HttpGet]
        public IActionResult CreateNews()
        {
            ViewData["Title"] = "Create New News";

            int userId = GetCurrentUserId();

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "Unable to identify user. Please log in again.";
                return RedirectToAction("News");
            }

            var model = new NewsCreateModel
            {
                UserId = userId
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNews(NewsCreateModel model, IFormFile? imageFile)
        {
            int staffUserId = GetCurrentUserId();
            if (staffUserId == 0)
            {
                TempData["ErrorMessage"] = "Unable to identify user. Please log in again.";
                return View(model);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please enter complete information.";
                    return View(model);
                }

                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(staffUserId.ToString()), "UserId");
                formData.Add(new StringContent(model.Title ?? ""), "Title");
                formData.Add(new StringContent(model.Content ?? ""), "Content");
                formData.Add(new StringContent(model.NewsStatus.ToString() ?? "Draft"), "NewsStatus");

                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageContent = new StreamContent(imageFile.OpenReadStream());
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                    formData.Add(imageContent, "ImageFile", imageFile.FileName);
                }

                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    return View(model);
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync("News", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "News created successfully!";
                    return RedirectToAction("News");
                }

                TempData["ErrorMessage"] = $"News creation failed: {responseContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "System error: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewNews(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    return RedirectToAction("News");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"News/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "News not found.";
                    return RedirectToAction("News");
                }

                var content = await response.Content.ReadAsStringAsync();
                var newsItem = JsonSerializer.Deserialize<EditNewsModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                ViewData["Title"] = $"View News Details - {newsItem?.Title}";
                return View("ViewNews", newsItem);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Server connection error: " + ex.Message;
                return RedirectToAction("News");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditNews(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    return RedirectToAction("News");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"News/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "News not found.";
                    return RedirectToAction("News");
                }

                var content = await response.Content.ReadAsStringAsync();
                var newsItem = JsonSerializer.Deserialize<EditNewsModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                ViewData["Title"] = $"Edit News - {newsItem?.Title}";
                return View(newsItem);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Server connection error: " + ex.Message;
                return RedirectToAction("News");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditNews(int id, EditNewsModel model, IFormFile? imageFile)
        {
            if (model.UserId == 0)
            {
                TempData["ErrorMessage"] = "Unable to identify user (UserId missing). Please try again.";
                return View(model);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please enter complete information.";
                    return View(model);
                }

                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(model.Id.ToString()), "Id");
                formData.Add(new StringContent(model.UserId.ToString()), "UserId");
                formData.Add(new StringContent(model.Title ?? ""), "Title");
                formData.Add(new StringContent(model.Content ?? ""), "Content");
                formData.Add(new StringContent(model.NewsStatus.ToString() ?? "Draft"), "NewsStatus");

                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageContent = new StreamContent(imageFile.OpenReadStream());
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                    formData.Add(imageContent, "ImageFile", imageFile.FileName);
                }

                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    return View(model);
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsync($"News/{id}", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "News updated successfully!";
                    return RedirectToAction("News");
                }

                TempData["ErrorMessage"] = $"Update failed: {responseContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "System error: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNews(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Login session has expired. Please log in again.";
                    return RedirectToAction("News");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"News/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "News deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "News deletion failed!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "System error: " + ex.Message;
            }

            return RedirectToAction("News");
        }

        private string GetTourImageUrl(TourViewModel tour)
        {
            if (tour.TourImages != null && tour.TourImages.Any())
            {
                var firstImage = tour.TourImages.First();

                if (firstImage.Image.StartsWith("data:image"))
                {
                    return firstImage.Image;
                }
                else if (!string.IsNullOrEmpty(firstImage.Image))
                {
                    return $"{BASE_API_URL}Tour/GetImage?path={Uri.EscapeDataString(firstImage.Image)}";
                }
            }

            return $"{BASE_API_URL}Tour/GetImage?path=images/default-tour.jpg";
        }

        private string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return $"{BASE_API_URL}Tour/GetImage?path=images/default-tour.jpg";

            if (imagePath.StartsWith("data:image"))
                return imagePath;

            if (imagePath.StartsWith("http"))
                return imagePath;

            return $"{BASE_API_URL}Tour/GetImage?path={Uri.EscapeDataString(imagePath)}";
        }
    }
}