using FirstWebApplication.DataContext;
using FirstWebApplication.DataContext.Seeders;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();
builder.Services.AddScoped<IObstacleRepository, ObstacleRepository>();
builder.Services.AddScoped<IRegistrarRepository, RegistrarRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// === DATABASE SETUP ===
var conn = builder.Configuration.GetConnectionString("DatabaseConnection");

var serverVersion = new MySqlServerVersion(new Version(11, 8, 3));

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(conn, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// === IDENTITY SETUP ===
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
});

// === COOKIES ===
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

// === SESSION ===
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// === APPLY MIGRATIONS AND SEED DATABASE ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDBContext>();
    
    // Wait for database to be ready with retry logic
    var maxRetries = 30;
    var retryDelay = TimeSpan.FromSeconds(2);
    var retryCount = 0;
    var databaseReady = false;
    
    while (retryCount < maxRetries && !databaseReady)
    {
        try
        {
            logger.LogInformation("Checking database connection... (Attempt {RetryCount}/{MaxRetries})", retryCount + 1, maxRetries);
            databaseReady = await context.Database.CanConnectAsync();
            
            if (databaseReady)
            {
                logger.LogInformation("Database connection successful!");
            }
            else
            {
                throw new Exception("Database connection failed");
            }
        }
        catch (Exception ex)
        {
            retryCount++;
            if (retryCount >= maxRetries)
            {
                logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts. Application will continue but migrations may fail.", maxRetries);
                break;
            }
            logger.LogWarning("Database not ready yet. Retrying in {Delay} seconds...", retryDelay.TotalSeconds);
            await Task.Delay(retryDelay);
        }
    }
    
    // Apply pending migrations
    if (databaseReady)
    {
        try
        {
            logger.LogInformation("Applying database migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Found {Count} pending migration(s): {Migrations}", 
                    pendingMigrations.Count(), 
                    string.Join(", ", pendingMigrations));
            }
            else
            {
                logger.LogInformation("No pending migrations. Database is up to date.");
            }
            
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw; // Re-throw to prevent app from starting with broken database
        }
        
        // Seed the database
        try
        {
            logger.LogInformation("Seeding database...");
            await AuthDbSeeder.SeedAsync(services);
            logger.LogInformation("Database seeding completed!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            // Don't throw here - seeding failures shouldn't prevent app from starting
        }
    }
}

// === SECURITY HEADERS ===
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
