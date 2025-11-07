using BE_Capstone_Project.Application.Auth.Services;
using BE_Capstone_Project.Application.Bookings.Services;
using BE_Capstone_Project.Application.CancelConditions.Services;
using BE_Capstone_Project.Application.CancelConditions.Services.Interfaces;
using BE_Capstone_Project.Application.Categories.Services;
using BE_Capstone_Project.Application.Categories.Services.Interfaces;
using BE_Capstone_Project.Application.Locations.Services;
using BE_Capstone_Project.Application.Locations.Services.Interfaces;
using BE_Capstone_Project.Application.Newses.Services;
using BE_Capstone_Project.Application.Notifications.Services;
using BE_Capstone_Project.Application.Payment.VnPayService;
using BE_Capstone_Project.Application.Payment.VnPayService.Interfaces;
using BE_Capstone_Project.Application.Report.Services;
using BE_Capstone_Project.Application.Report.Services.Interfaces;
using BE_Capstone_Project.Application.ReviewManagement.Services;
using BE_Capstone_Project.Application.ReviewManagement.Services.Interfaces;
using BE_Capstone_Project.Application.Services;
using BE_Capstone_Project.Application.TourManagement.Services;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.Application.TourPriceHistories.Services;
using BE_Capstone_Project.Application.WishlistManagement.Services;
using BE_Capstone_Project.Application.WishlistManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<OtmsdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<ITourImageService, TourImageService>();
builder.Services.AddScoped<ITourScheduleService, TourScheduleService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TourPriceHistoryService>();
builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ITourCategoryService, TourCategoryService>();
builder.Services.AddScoped<ICancelConditionService, CancelConditionService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

//DAO
builder.Services.AddScoped<BookingCustomerDAO>();
builder.Services.AddScoped<BookingDAO>();
builder.Services.AddScoped<CancelConditionDAO>();
builder.Services.AddScoped<ChatDAO>();
builder.Services.AddScoped<LocationDAO>();
builder.Services.AddScoped<NewsDAO>();
builder.Services.AddScoped<NotificationDAO>();
builder.Services.AddScoped<ReviewDAO>();
builder.Services.AddScoped<RoleDAO>();
builder.Services.AddScoped<TourCategoryDAO>();
builder.Services.AddScoped<TourDAO>();
builder.Services.AddScoped<TourImageDAO>();
builder.Services.AddScoped<TourPriceHistoryDAO>();
builder.Services.AddScoped<TourScheduleDAO>();
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<WishlistDAO>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()      
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();
app.UseCors("AllowAll");
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/payment/payment-callback"))
    {
        // Enable buffering for request body
        context.Request.EnableBuffering();

        // Log raw request data
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var bodyStr = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0; // Reset position

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Payment callback raw data: {Data}", bodyStr);
    }

    await next();
});
if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
