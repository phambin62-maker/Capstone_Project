using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Models;
using FE_Capstone_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Controllers
{
    //[Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffController> _logger;
        private const string BASE_API_URL = "https://localhost:7160/api/";
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DataService _dataService;

        public StaffController(IHttpClientFactory httpClientFactory, ILogger<StaffController> logger, DataService dataService)
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

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"API Call: {endpoint}, Status: {response.StatusCode}, Response: {responseContent}");

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
                    _logger.LogWarning("API call failed: {Endpoint}, Status: {StatusCode}, Response: {Response}",
                        endpoint, response.StatusCode, responseContent);
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
            ViewData["Title"] = "Quản lý Tour";

            try
            {
                // Chuyển đổi string status sang bool?
                bool? statusFilter = null;
                if (!string.IsNullOrEmpty(status))
                {
                    if (status.ToLower() == "active")
                        statusFilter = true;
                    else if (status.ToLower() == "inactive")
                        statusFilter = false;
                }

                // Xây dựng URL API với các tham số filter
                var apiUrl = $"Tour/GetFilteredTours?page={page}&pageSize={pageSize}";

                // Thêm các tham số filter nếu có
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
                    ViewBag.ErrorMessage = $"Không thể tải danh sách tour: {error}";
                    return View(new List<TourViewModel>());
                }

                var tours = toursResponse?.Tours ?? new List<TourViewModel>();
                tours = await LoadToursWithDetails(tours);

                var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
                var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");

                // Lấy tổng số count với filter
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

                // Lưu các tham số filter hiện tại để hiển thị trong view
                ViewBag.CurrentStatus = status;
                ViewBag.CurrentStartLocation = startLocation;
                ViewBag.CurrentEndLocation = endLocation;
                ViewBag.CurrentCategory = category;
                ViewBag.CurrentMinPrice = minPrice;
                ViewBag.CurrentMaxPrice = maxPrice;
                ViewBag.CurrentSort = sort;
                ViewBag.CurrentSearch = search;

                return View(tours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tours");
                ViewBag.ErrorMessage = "Lỗi kết nối đến server. Vui lòng kiểm tra lại kết nối.";
                return View(new List<TourViewModel>());
            }
        }

        // FIXED: TourDetails method
        public async Task<IActionResult> TourDetails(int id)
        {
            try
            {
                _logger.LogInformation($"Loading tour details for ID: {id}");

                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{id}");

                if (!success || result?.Tour == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy tour với ID {id}. Lỗi: {error}";
                    return RedirectToAction("Tours");
                }

                // Tạo TourDetailModel từ Tour
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

                ViewData["Title"] = $"Chi tiết Tour - {tourDetail.Name}";
                return View(tourDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tour details for ID: {TourId}", id);
                TempData["ErrorMessage"] = $"Lỗi kết nối đến server: {ex.Message}";
                return RedirectToAction("Tours");
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation($"Loading tour for edit ID: {id}");

                // Lấy dữ liệu tour chi tiết
                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{id}");
                if (!success || result?.Tour == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy tour với ID {id}. Lỗi: {error}";
                    return RedirectToAction("Tours");
                }

                var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
                var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");
                var (cancelSuccess, cancelConditions, _) = await CallApiAsync<List<CancelCondition>>("CancelCondition");



                ViewBag.Locations = locSuccess ? locations : new List<Location>();
                ViewBag.Categories = catSuccess ? categories : new List<TourCategory>();
                ViewBag.CancelConditions = cancelSuccess ? cancelConditions : new List<CancelCondition>();

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

                ViewData["Title"] = $"Chỉnh sửa Tour - {editModel.Name}";
                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tour for edit ID: {TourId}", id);
                TempData["ErrorMessage"] = "Lỗi kết nối đến server.";
                return RedirectToAction("Tours");
            }
        }


        
        [HttpPost]
        public async Task<IActionResult> Edit(TourEditModel model)
        {
            _logger.LogInformation($"Updating tour ID: {model.Id}");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                var formData = CreateTourFormData(model);

                // SỬA: Gọi API UpdateTour
                var response = await _httpClient.PostAsync("Tour/UpdateTour", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"UpdateTour Response: {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật tour thành công!";
                    return RedirectToAction("Tours");
                }
                else
                {
                    var errorMessage = $"Cập nhật tour thất bại! Status: {response.StatusCode}";
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
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tour ID: {TourId}", model.Id);
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(TourCreateModel model)
        {
            _logger.LogInformation("Creating new tour");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for tour creation");
                return View(model);
            }

            try
            {
                var formData = CreateTourFormData(model);
                var response = await _httpClient.PostAsync("Tour/AddTour", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"AddTour Response: {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm tour thành công!";
                    return RedirectToAction("Tours");
                }
                else
                {
                    var errorMessage = $"Thêm tour thất bại! Status: {response.StatusCode}";
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
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tour");
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
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
                    setNameAction("Không tìm thấy");
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

                setNameAction(name ?? "Không có tên");
            }
            catch (Exception ex)
            {
                setNameAction($"Lỗi: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteTour(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Tour/DeleteTour?tourId={id}");
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"DeleteTour Response: {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa tour thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Xóa tour thất bại!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tour ID: {TourId}", id);
                TempData["ErrorMessage"] = "Lỗi kết nối đến server!";
            }

            return RedirectToAction("Tours");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTourStatus(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"Tour/ToggleTourStatus?tourId={id}", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"ToggleTourStatus Response: {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TourStatusResponse>(responseContent, _jsonOptions);
                    TempData["SuccessMessage"] = result?.Message ?? "Thay đổi trạng thái thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Thay đổi trạng thái tour thất bại!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling tour status ID: {TourId}", id);
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
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

            if (model is TourEditModel editModel)
            {
                formData.Add(new StringContent(editModel.Id.ToString()), "Id");
            }

            if (model.Images != null && model.Images.Count > 0)
            {
                foreach (var image in model.Images.Where(img => img.Length > 0))
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
            ViewData["Title"] = "Thêm Tour Mới";

            var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
            var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");
            var (cancelSuccess, cancelConditions, _) = await CallApiAsync<List<CancelCondition>>("CancelCondition");



            ViewBag.Locations = locSuccess ? locations : new List<Location>();
            ViewBag.Categories = catSuccess ? categories : new List<TourCategory>();
            ViewBag.CancelConditions = cancelSuccess ? cancelConditions : new List<CancelCondition>();

            return View();
        }


        public IActionResult Blog()
        {
            ViewData["Title"] = "Hồ sơ cá nhân";
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

        public async Task<IActionResult> News(int page = 1, int pageSize = 10, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
        {
            ViewData["Title"] = "Quản lý Tin tức";

            try
            {
                var response = await _httpClient.GetAsync($"News");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách tin tức.";
                    return View(new NewsListViewModel { NewsList = new List<NewsViewModel>() });
                }

                var content = await response.Content.ReadAsStringAsync();

                var newsList = JsonSerializer.Deserialize<List<NewsViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<NewsViewModel>();

                IEnumerable<NewsViewModel> filteredNews = newsList;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string normalizedSearch = NormalizeString(search);

                    filteredNews = filteredNews.Where(n =>
                    {
                        string normalizedTitle = NormalizeString(n.Title);
                        string normalizedAuthor = NormalizeString(n.AuthorName);
                        string normalizedContent = NormalizeString(n.Content);

                        return normalizedTitle.Contains(normalizedSearch) ||
                               normalizedAuthor.Contains(normalizedSearch) ||
                               normalizedContent.Contains(normalizedSearch);
                    }).ToList();
                }

                if (fromDate.HasValue)
                {
                    filteredNews = filteredNews.Where(n => n.CreatedDate?.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    filteredNews = filteredNews.Where(n => n.CreatedDate?.Date <= toDate.Value.Date);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    string lowerStatus = status.ToLowerInvariant();
                    filteredNews = filteredNews.Where(n => n.NewsStatus != null && n.NewsStatus.ToLowerInvariant() == lowerStatus);
                }

                var finalNewsList = filteredNews.ToList();
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
                TempData["ErrorMessage"] = "Lỗi kết nối đến server: " + ex.Message;
                return View(new NewsListViewModel { NewsList = new List<NewsViewModel>() });
            }
        }

        [HttpGet]
        public IActionResult CreateNews()
        {
            ViewData["Title"] = "Tạo Tin tức Mới";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNews(NewsCreateModel model, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                    return View(model);
                }

                string? imageBase64 = null;
                if (imageFile != null && imageFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await imageFile.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    imageBase64 = $"data:{imageFile.ContentType};base64,{Convert.ToBase64String(imageBytes)}";
                }


                var dto = new
                {
                    UserId = model.UserId,
                    Title = model.Title,
                    Content = model.Content,
                    Image = imageBase64,
                    NewsStatus = model.NewsStatus
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("News", jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tạo tin tức thành công!";
                    return RedirectToAction("News");
                }

                TempData["ErrorMessage"] = $"Tạo tin thất bại: {responseContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewNews(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"News/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin tức.";
                    return RedirectToAction("News");
                }

                var content = await response.Content.ReadAsStringAsync();

                var newsItem = JsonSerializer.Deserialize<EditNewsModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                ViewData["Title"] = $"Xem chi tiết Tin - {newsItem?.Title}";
                return View("ViewNews", newsItem);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server: " + ex.Message;
                return RedirectToAction("News");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditNews(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"News/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tin tức.";
                    return RedirectToAction("News");
                }

                var content = await response.Content.ReadAsStringAsync();

                var newsItem = JsonSerializer.Deserialize<EditNewsModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                ViewData["Title"] = $"Chỉnh sửa Tin - {newsItem?.Title}";
                return View(newsItem);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server: " + ex.Message;
                return RedirectToAction("News");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditNews(int id, NewsCreateModel model, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                    return View(model);
                }

                string? imageBase64 = model.Image;
                if (imageFile != null && imageFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await imageFile.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    imageBase64 = $"data:{imageFile.ContentType};base64,{Convert.ToBase64String(imageBytes)}";
                }

                var dto = new
                {
                    UserId = model.UserId,
                    Title = model.Title,
                    Content = model.Content,
                    Image = imageBase64,
                    NewsStatus = model.NewsStatus
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"News/{id}", jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật tin tức thành công!";
                    return RedirectToAction("News");
                }

                TempData["ErrorMessage"] = $"Cập nhật thất bại: {responseContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNews(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"News/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa tin tức thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Xóa tin thất bại!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("News");
        }
        private string GetTourImageUrl(TourViewModel tour)
        {
            if (tour.TourImages != null && tour.TourImages.Any())
            {
                var firstImage = tour.TourImages.First();

                // Nếu image là base64 string
                if (firstImage.Image.StartsWith("data:image"))
                {
                    return firstImage.Image;
                }
                // Nếu image là đường dẫn tương đối
                else if (!string.IsNullOrEmpty(firstImage.Image))
                {
                    // Sử dụng endpoint mới từ BE để lấy ảnh
                    return $"{BASE_API_URL}Tour/GetImage?path={Uri.EscapeDataString(firstImage.Image)}";
                }
            }

            // Trả về ảnh mặc định
            return $"{BASE_API_URL}Tour/GetImage?path=images/default-tour.jpg";
        }

        // Phương thức helper để lấy URL ảnh từ đường dẫn
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