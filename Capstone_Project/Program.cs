using BE_Capstone_Project.Application.Auth.Services;
using BE_Capstone_Project.Application.Report.Services;
using BE_Capstone_Project.Application.Report.Services.Interfaces;
using BE_Capstone_Project.Application.TourManagement.Services;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<OtmsdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<ITourImageService, TourImageService>();

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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

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
