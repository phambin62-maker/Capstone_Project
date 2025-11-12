namespace FE_Capstone_Project.Models
{
    public class NewsCreateModel
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public string? NewsStatus { get; set; }
    }

    public class EditNewsModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? NewsStatus { get; set; }
    }

    public class NewsViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public string? AuthorName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? NewsStatus { get; set; }
        public string? NewsTitle { get; set; }
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
    }

        public class NewsListViewModel
        {
            public List<NewsViewModel> NewsList { get; set; } = new();
            public string? Search { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public string? Status { get; set; }
        }


    public class NewsStatsDTO
    {
        public int Total { get; set; }
        public int Published { get; set; }
        public int Draft { get; set; }
        public int Hidden { get; set; }
    }
}
