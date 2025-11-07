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


var conn = builder.Configuration.GetConnectionString("DatabaseConnection");
var serverVersion = new MySqlServerVersion(new Version(11, 8, 3));

// Application DB with transient-failure retry policy
// >>> ADD: Bruker-repo for AdminController
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DatabaseConnection"),
    new MySqlServerVersion(new Version(11, 8, 3))));

// Application DB with transient-failure retry policy
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(conn, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,                       // number of retries
            maxRetryDelay: TimeSpan.FromSeconds(30),// delay between retries
            errorNumbersToAdd: null)));             // additional MySQL error codes

// Auth DB for Identity with the same retry policy
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(conn, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));



// Optional: configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});


builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("AuthConnection"),
    new MySqlServerVersion(new Version(11, 8, 3))));

// Configure Identity and password policy in AddIdentity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<ApplicationContext>()
.AddDefaultTokenProviders();

// >>> (Valgfritt) legg på en Admin-policy hvis du vil bruke [Authorize(Policy="AdminOnly")]
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

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
    var context = services.GetRequiredService<ApplicationContext>();
    AuthDbSeeder.Seed(context);
}
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
// Mapper statiske filer (CSS, JS, bilder osv.)
    app.MapStaticAssets();
    app.UseAuthentication();
    app.UseAuthorization();

// >>> (Valgfritt) egen rute til Admin (da blir /admin kortvei til Dashboard)
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Setter opp standard routing for controllerne
    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

app.MapRazorPages();

app.Run();