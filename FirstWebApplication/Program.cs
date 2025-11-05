using FirstWebApplication.DataContext;
using FirstWebApplication.DataContext.Seeders;
using FirstWebApplication.Models;
using FirstWebApplication.NewFolder;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Identity;    
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();
builder.Services.AddScoped<IObstacleRepository, ObstacleRepository>();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DatabaseConnection"),
    new MySqlServerVersion(new Version(11, 8, 3))));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("AuthConnection"),
    new MySqlServerVersion(new Version(11, 8, 3))));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>().AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(options =>
{
    //Default settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AuthDbContext>();
    AuthDbSeeder.Seed(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication(); //todo: hva gjør usesession!!
app.UseAuthorization();

// Mapper statiske filer (CSS, JS, bilder osv.)
app.MapStaticAssets();

// Setter opp standard routing for controllerne
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
