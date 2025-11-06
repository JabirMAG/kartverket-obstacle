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
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DatabaseConnection"),
    new MySqlServerVersion(new Version(11, 8, 3))));

// Application DB with transient-failure retry policy
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(conn, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,                       // number of retries
            maxRetryDelay: TimeSpan.FromSeconds(30),// delay between retries
            errorNumbersToAdd: null)));             // additional MySQL error codes

// Auth DB for Identity with the same retry policy
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(conn, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Add Identity (this configures default authentication schemes)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Optional: configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});


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
    var context = services.GetRequiredService<AuthDbContext>();
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
    app.UseAuthentication(); //todo: hva gjør usesession!!
    app.UseAuthorization();

// Mapper statiske filer (CSS, JS, bilder osv.)
    app.MapStaticAssets();

// Setter opp standard routing for controllerne
    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

app.MapRazorPages();

app.Run();