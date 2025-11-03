using FE_Capstone_Project.Models;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
namespace FE_Capstone_Project.Controllers
{
    //[Authorize] // Nếu cần authentication
    public class StaffController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7160/api/";

        public StaffController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BASE_API_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Staff Dashboard";
            return View();
        }

        public IActionResult Tours(int page = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Quản lý Tour";

            try
            {
                var response = _httpClient.GetAsync($"{BASE_API_URL}Tour/GetPaginatedTours?page={page}&pageSize={pageSize}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonSerializer.Deserialize<TourListResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


                    var tours = result?.Tours ?? new List<TourViewModel>();

                    var countResponse = _httpClient.GetAsync($"{BASE_API_URL}Tour/GetTotalTourCount").Result;
                    var totalCount = 0;

                    if (countResponse.IsSuccessStatusCode)
                    {
                        var countContent = countResponse.Content.ReadAsStringAsync().Result;
                        var countResult = JsonSerializer.Deserialize<TourCountResponse>(countContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        totalCount = countResult?.TourCount ?? tours.Count;
                    }
                    else
                    {
                        totalCount = tours.Count;
                    }

                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalCount = totalCount;
                    ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    return View(tours);
                }
                else
                {
                    ViewBag.ErrorMessage = "Không thể tải danh sách tour. Vui lòng thử lại sau.";
                    return View(new List<TourViewModel>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi kết nối đến server. Vui lòng kiểm tra lại kết nối.";
                return View(new List<TourViewModel>());
            }
        }

        public async Task<IActionResult> TourDetails(int id)
        {
            try
            {
                var tourResponse = await _httpClient.GetAsync($"Tour/GetTourById?id={id}");

                if (tourResponse.IsSuccessStatusCode)
                {
                    var tourContent = await tourResponse.Content.ReadAsStringAsync();
                    var tourResult = JsonSerializer.Deserialize<TourDetailResponse>(tourContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (tourResult?.Tour != null)
                    {
                        var tourDetail = new TourDetailModel
                        {
                            Id = tourResult.Tour.Id,
                            Name = tourResult.Tour.Name,
                            Description = tourResult.Tour.Description,
                            Price = tourResult.Tour.Price,
                            Duration = tourResult.Tour.Duration,
                            StartLocationId = tourResult.Tour.StartLocationId,
                            EndLocationId = tourResult.Tour.EndLocationId,
                            CategoryId = tourResult.Tour.CategoryId,
                            CancelConditionId = tourResult.Tour.CancelConditionId,
                            ChildDiscount = tourResult.Tour.ChildDiscount,
                            GroupDiscount = tourResult.Tour.GroupDiscount,
                            GroupNumber = tourResult.Tour.GroupNumber,
                            MinSeats = tourResult.Tour.MinSeats,
                            MaxSeats = tourResult.Tour.MaxSeats,
                            TourStatus = tourResult.Tour.TourStatus
                        };

                        try
                        {
                            var startLocationResponse = await _httpClient.GetAsync($"Location/GetLocationById?id={tourResult.Tour.StartLocationId}");
                            if (startLocationResponse.IsSuccessStatusCode)
                            {
                                var locationContent = await startLocationResponse.Content.ReadAsStringAsync();
                                var locationResult = JsonSerializer.Deserialize<dynamic>(locationContent);
                                tourDetail.StartLocationName = locationResult?.GetProperty("name").GetString() ?? "Không xác định";
                            }

                            var endLocationResponse = await _httpClient.GetAsync($"Location/GetLocationById?id={tourResult.Tour.EndLocationId}");
                            if (endLocationResponse.IsSuccessStatusCode)
                            {
                                var locationContent = await endLocationResponse.Content.ReadAsStringAsync();
                                var locationResult = JsonSerializer.Deserialize<dynamic>(locationContent);
                                tourDetail.EndLocationName = locationResult?.GetProperty("name").GetString() ?? "Không xác định";
                            }

                            var categoryResponse = await _httpClient.GetAsync($"Category/GetCategoryById?id={tourResult.Tour.CategoryId}");
                            if (categoryResponse.IsSuccessStatusCode)
                            {
                                var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
                                var categoryResult = JsonSerializer.Deserialize<dynamic>(categoryContent);
                                tourDetail.CategoryName = categoryResult?.GetProperty("name").GetString() ?? "Không xác định";
                            }

                            var cancelConditionResponse = await _httpClient.GetAsync($"CancelCondition/GetCancelConditionById?id={tourResult.Tour.CancelConditionId}");
                            if (cancelConditionResponse.IsSuccessStatusCode)
                            {
                                var cancelContent = await cancelConditionResponse.Content.ReadAsStringAsync();
                                var cancelResult = JsonSerializer.Deserialize<dynamic>(cancelContent);
                                tourDetail.CancelConditionName = cancelResult?.GetProperty("name").GetString() ?? "Không xác định";
                            }
                        }
                        catch (Exception ex)
                        {
                            tourDetail.StartLocationName = "ID: " + tourResult.Tour.StartLocationId;
                            tourDetail.EndLocationName = "ID: " + tourResult.Tour.EndLocationId;
                            tourDetail.CategoryName = "ID: " + tourResult.Tour.CategoryId;
                            tourDetail.CancelConditionName = "ID: " + tourResult.Tour.CancelConditionId;
                        }

                        ViewData["Title"] = $"Chi tiết Tour - {tourDetail.Name}";
                        return View(tourDetail);
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy tour.";
                return RedirectToAction("Tours");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server.";
                return RedirectToAction("Tours");
            }
        }

        [HttpPost]
        public IActionResult DeleteTour(int id)
        {
            try
            {
                var response = _httpClient.DeleteAsync($"{BASE_API_URL}Tour/DeleteTour?tourId={id}").Result;

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
                TempData["ErrorMessage"] = "Lỗi kết nối đến server!";
            }

            return RedirectToAction("Tours");
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm Tour Mới";
            return View();
        }

        // CREATE - Xử lý tạo tour (Sửa đúng format form data)
        [HttpPost]
        public async Task<IActionResult> Create(TourCreateModel model, List<IFormFile> images)
        {
            try
            {
                Console.WriteLine("=== CREATE TOUR ===");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState Invalid");
                    return View(model);
                }

                Console.WriteLine("ModelState Valid");

                // Tạo form data đúng format API BE mong đợi
                var formData = new MultipartFormDataContent();

                // Thêm các field của tour riêng lẻ (không phải JSON object)
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

                Console.WriteLine("Added tour fields to form data");

                // Thêm images nếu có
                if (images != null && images.Count > 0)
                {
                    Console.WriteLine($"Adding {images.Count} images");
                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await image.CopyToAsync(memoryStream);
                            var imageBytes = memoryStream.ToArray();
                            var base64String = Convert.ToBase64String(imageBytes);
                            var imageData = $"data:{image.ContentType};base64,{base64String}";
                            formData.Add(new StringContent(imageData), "images");
                            Console.WriteLine($"Added image: {image.FileName}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No images, adding empty images array");
                    formData.Add(new StringContent(""), "images");
                }

                Console.WriteLine("Calling API...");

                // Gọi API
                var response = await _httpClient.PostAsync("Tour/AddTour", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm tour thành công!";
                    return RedirectToAction("Tours");
                }
                else
                {
                    TempData["ErrorMessage"] = $"Thêm tour thất bại! Lỗi: {responseContent}";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex}");
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return View(model);
            }
        }

        
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Tour/GetTourById?id={id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TourDetailResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Tour != null)
                    {
                        ViewData["Title"] = $"Chỉnh sửa Tour - {result.Tour.Name}";

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

                        return View(editModel);
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy tour.";
                return RedirectToAction("Tours");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server.";
                return RedirectToAction("Tours");
            }
        }

        // EDIT - Xử lý cập nhật tour
        [HttpPost]
        public async Task<IActionResult> Edit(TourEditModel model, List<IFormFile> images)
        {
            try
            {
                Console.WriteLine("=== EDIT TOUR ===");
                Console.WriteLine($"Model valid: {ModelState.IsValid}");
                Console.WriteLine($"Tour ID: {model.Id}");

                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
                    return View(model);
                }

                // Tạo form data
                var formData = new MultipartFormDataContent();

                // Thêm các field của tour riêng lẻ
                formData.Add(new StringContent(model.Id.ToString()), "Id");
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

                Console.WriteLine("Added tour fields to form data");

                // Thêm images nếu có
                if (images != null && images.Count > 0)
                {
                    Console.WriteLine($"Adding {images.Count} images");
                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await image.CopyToAsync(memoryStream);
                            var imageBytes = memoryStream.ToArray();
                            var base64String = Convert.ToBase64String(imageBytes);
                            var imageData = $"data:{image.ContentType};base64,{base64String}";
                            formData.Add(new StringContent(imageData), "images");
                            Console.WriteLine($"Added image: {image.FileName}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No images, adding empty images");
                    formData.Add(new StringContent(""), "images");
                }

                Console.WriteLine("Calling UpdateTour API...");

                // Gọi API UpdateTour
                var response = await _httpClient.PostAsync("Tour/UpdateTour", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật tour thành công!";
                    return RedirectToAction("Tours");
                }
                else
                {
                    TempData["ErrorMessage"] = $"Cập nhật tour thất bại! Lỗi: {responseContent}";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex}");
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return View(model);
            }
        }

        // Sửa lại action ToggleTourStatus trong StaffController (FE)
        [HttpPost]
        public async Task<IActionResult> ToggleTourStatus(int id)
        {
            try
            {
                // Gọi API toggle status
                var response = await _httpClient.PostAsync($"Tour/ToggleTourStatus?tourId={id}", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TourStatusResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["SuccessMessage"] = result?.Message ?? "Thay đổi trạng thái thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Thay đổi trạng thái tour thất bại!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            return RedirectToAction("Tours");
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
    }
}
