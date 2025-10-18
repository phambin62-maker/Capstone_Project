using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Application.Newses.Services;
using BE_Capstone_Project.Application.Tour.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OtmsdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TourDAO>();
builder.Services.AddScoped<NewsDAO>();

builder.Services.AddScoped<TourService>();
builder.Services.AddScoped<NewsService>();


builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true; 
})
.AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.WriteIndented = true;
})
.AddXmlSerializerFormatters(); 


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles(); 

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.Run();
