using FirstWebApplication.DataContext;
using FirstWebApplication.DataContext.Seeders;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

/// <summary>
/// Application entry point and configuration. Sets up services, middleware, and routing
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Configure MVC with automatic CSRF protection for all POST requests
/// </summary>
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRazorPages();

/// <summary>
/// Register repositories for dependency injection
/// </summary>
builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();
builder.Services.AddScoped<IObstacleRepository, ObstacleRepository>();
builder.Services.AddScoped<IRegistrarRepository, RegistrarRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IArchiveRepository, ArchiveRepository>();

/// <summary>
/// Database setup: Configure MySQL connection with retry logic
/// </summary>
var conn = builder.Configuration.GetConnectionString("DatabaseConnection");
if (string.IsNullOrEmpty(conn))
{
    throw new InvalidOperationException("Connection string 'DatabaseConnection' is not configured.");
}

var serverVersion = new MySqlServerVersion(new Version(11, 8, 3));

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(conn, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

/// <summary>
/// Identity setup: Configure ASP.NET Identity for user authentication and authorization
/// </summary>
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

/// <summary>
/// Password policy configuration: Require strong passwords
/// </summary>
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
});

/// <summary>
/// Cookie configuration: Set login paths and session timeout
/// </summary>
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

/// <summary>
/// Session configuration: Configure distributed memory cache and session settings
/// </summary>
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

/// <summary>
/// Apply database migrations automatically on startup
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDBContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

/// <summary>
/// Seed database with initial data (roles and admin user)
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    AuthDbSeeder.SeedAsync(services).GetAwaiter().GetResult();
}

/// <summary>
/// Security headers: Add security headers to all HTTP responses
/// </summary>
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

/// <summary>
/// Error handling: Use custom error page in production, enable HSTS
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

/// <summary>
/// Middleware pipeline: Configure request processing order
/// </summary>
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
// Configure to listen on all network interfaces for iPad access

//if (app.Environment.IsDevelopment())
//{
//    app.Urls.Clear();
//    app.Urls.Add("http://0.0.0.0:5193");
//}

/// <summary>
/// Routing: Configure default MVC routing
/// </summary>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
