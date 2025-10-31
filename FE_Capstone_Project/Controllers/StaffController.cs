using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class StaffController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffController> _logger;
        private const string BASE_API_URL = "https://localhost:7160/api/";
        private readonly JsonSerializerOptions _jsonOptions;

        public StaffController(IHttpClientFactory httpClientFactory, ILogger<StaffController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BASE_API_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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

                    // ✅ Nếu là object và có "data" thì deserialize phần data
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataElement))
                    {
                        var result = JsonSerializer.Deserialize<T>(dataElement.GetRawText(), _jsonOptions);
                        return (true, result, string.Empty);
                    }
                    // ✅ Nếu là mảng hoặc object không có "data"
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

        public async Task<IActionResult> Tours(int page = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Quản lý Tour";

            try
            {
                var (success, toursResponse, error) = await CallApiAsync<TourListResponse>($"Tour/GetPaginatedTours?page={page}&pageSize={pageSize}");

                if (!success)
                {
                    ViewBag.ErrorMessage = $"Không thể tải danh sách tour: {error}";
                    return View(new List<TourViewModel>());
                }

                var tours = toursResponse?.Tours ?? new List<TourViewModel>();

                // Get total count
                var (countSuccess, countResponse, countError) = await CallApiAsync<TourCountResponse>("Tour/GetTotalTourCount");
                var totalCount = countSuccess ? countResponse?.TourCount ?? tours.Count : tours.Count;

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

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

                // Load additional info với xử lý lỗi
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


        // FIXED: Edit method
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation($"Loading tour for edit ID: {id}");

                var (success, result, error) = await CallApiAsync<TourDetailResponse>($"Tour/GetTourById/{id}");

                if (!success || result?.Tour == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy tour với ID {id}. Lỗi: {error}";
                    return RedirectToAction("Tours");
                }

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

        // FIXED: Edit POST method
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

        // FIXED: Create method với logging
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

        private async Task LoadAdditionalInfo(TourDetailModel tourDetail)
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

        private async Task LoadNameFromApi(string endpoint, Action<string> setNameAction)
        {
            try
            {
                var (success, response, error) = await CallApiAsync<JsonElement>(endpoint);
                if (!success || response.ValueKind == JsonValueKind.Null)
                {
                    setNameAction("Không tìm thấy");
                    return;
                }

                // Một số API trả data trực tiếp (Location, Category)
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

        private MultipartFormDataContent CreateTourFormData(TourCreateModel model)
        {
            var formData = new MultipartFormDataContent();

            // Add basic tour fields - ĐÚNG với BE expectation
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
                    using var memoryStream = new MemoryStream();
                    image.CopyTo(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    var base64String = Convert.ToBase64String(imageBytes);
                    var imageData = $"data:{image.ContentType};base64,{base64String}";
                    formData.Add(new StringContent(imageData), "images");
                }
            }
            else
            {
                formData.Add(new StringContent(""), "images");
            }

            return formData;
        }
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm Tour Mới";

            // Lấy dữ liệu từ API
            var (locSuccess, locations, _) = await CallApiAsync<List<Location>>("Locations");
            var (catSuccess, categories, _) = await CallApiAsync<List<TourCategory>>("TourCategories");
            var (cancelSuccess, cancelConditions, _) = await CallApiAsync<List<CancelCondition>>("CancelCondition");



            // Gán vào ViewBag để View hiển thị dropdown
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
    }
}