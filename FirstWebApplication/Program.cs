using FirstWebApplication.DataContext;
using FirstWebApplication.DataContext.Seeders;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();
builder.Services.AddScoped<IObstacleRepository, ObstacleRepository>();
builder.Services.AddScoped<IRegistrarRepository, RegistrarRepository>();

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
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// >>> (Valgfritt) legg på en Admin-policy hvis du vil bruke [Authorize(Policy="AdminOnly")]
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
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

// === RUN MIGRATIONS ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDBContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // Retry logic to wait for database to be ready
    var maxRetries = 30;
    var retryDelay = TimeSpan.FromSeconds(2);
    var retryCount = 0;
    var migrationSucceeded = false;
    
    while (retryCount < maxRetries && !migrationSucceeded)
    {
        try
        {
            logger.LogInformation("Attempting to connect to database (attempt {Attempt}/{MaxAttempts})...", retryCount + 1, maxRetries);
            
            // Test database connection
            await context.Database.CanConnectAsync();
            
            logger.LogInformation("Database connection successful. Running migrations...");
            await context.Database.MigrateAsync();
            
            migrationSucceeded = true;
            logger.LogInformation("Migrations completed successfully.");
        }
        catch (Exception ex)
        {
            retryCount++;
            if (retryCount >= maxRetries)
            {
                logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts. Application will start but may not function correctly.", maxRetries);
                throw;
            }
            
            logger.LogWarning(ex, "Database connection failed. Retrying in {Delay} seconds... (attempt {Attempt}/{MaxAttempts})", retryDelay.TotalSeconds, retryCount, maxRetries);
            await Task.Delay(retryDelay);
        }
    }
}

// === SEED DATABASE ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await AuthDbSeeder.SeedAsync(services);
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
