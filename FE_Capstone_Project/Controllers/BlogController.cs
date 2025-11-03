using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text; 
using System.Globalization; 

namespace FE_Capstone_Project.Controllers
{
    [Route("Bolg")]
    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7160/api/News";

        public BlogController()
        {
            _httpClient = new HttpClient();
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


        [HttpGet]
        public async Task<IActionResult> Index(string? search, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 6)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Không thể tải danh sách bài viết.";
                    return View("Blog", new NewsListViewModel { NewsList = new List<NewsViewModel>() });
                }

                var json = await response.Content.ReadAsStringAsync();
                var newsList = JsonSerializer.Deserialize<List<NewsViewModel>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<NewsViewModel>();

                var publishedNews = newsList
                    .Where(n => n.NewsStatus == "1" || n.NewsStatus?.ToLower() == "published")
                    .OrderByDescending(n => n.CreatedDate)
                    .ToList();

                string normalizedSearch = NormalizeString(search);

                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    publishedNews = publishedNews
                        .Where(n =>
                        {
                            string normalizedTitle = NormalizeString(n.Title);
                            string normalizedAuthor = NormalizeString(n.AuthorName);
                            string normalizedContent = NormalizeString(n.Content);

                            return normalizedTitle.Contains(normalizedSearch) ||
                                   normalizedAuthor.Contains(normalizedSearch) ||
                                   normalizedContent.Contains(normalizedSearch);
                        })
                        .ToList();
                }

                if (fromDate.HasValue)
                    publishedNews = publishedNews.Where(n => n.CreatedDate >= fromDate.Value).ToList();

                if (toDate.HasValue)
                    publishedNews = publishedNews.Where(n => n.CreatedDate <= toDate.Value).ToList();

                int totalItems = publishedNews.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var pagedNews = publishedNews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var model = new NewsListViewModel
                {
                    NewsList = pagedNews,
                    Search = search,
                    FromDate = fromDate,
                    ToDate = toDate,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View("Blog", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tải dữ liệu: " + ex.Message;
                return View("Blog", new NewsListViewModel { NewsList = new List<NewsViewModel>() });
            }
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể tải bài viết.";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<NewsViewModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (model == null)
                {
                    TempData["ErrorMessage"] = "Bài viết không tồn tại.";
                    return RedirectToAction("Index");
                }

                return View("BlogDetail", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải bài viết: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Blog")]
        public Task<IActionResult> Blog(string? search, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 6)
        {
            return Index(search, fromDate, toDate, page, pageSize);
        }
    }
}