using BE_Capstone_Project.Infrastructure;
using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddScoped<DataService>();
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(apm => 
    {
        var backendAssembly = apm.ApplicationParts
            .FirstOrDefault(p => p.Name == "BE_Capstone_Project");
        if (backendAssembly != null)
        {
            apm.ApplicationParts.Remove(backendAssembly);
        }
    });
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FE_Capstone_Project.Helpers.AuthHelper>();
builder.Services.AddHttpClient<ApiHelper>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7160/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddDbContext<OtmsdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/AuthWeb/Login";
    options.AccessDeniedPath = "/Home/Forbidden";
})

.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});
var app = builder.Build();
builder.Services.AddAuthorization();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
