using FirstWebApplication.DataContext;
using FirstWebApplication.DataContext.Seeders;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

// Applikasjonsinngangspunkt og konfigurasjon. Setter opp tjenester, middleware og ruting
var builder = WebApplication.CreateBuilder(args);

// Konfigurerer MVC med automatisk CSRF-beskyttelse for alle POST-forespørsler
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRazorPages();

// Registrerer repositories for dependency injection
builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();
builder.Services.AddScoped<IObstacleRepository, ObstacleRepository>();
builder.Services.AddScoped<IRegistrarRepository, RegistrarRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IArchiveRepository, ArchiveRepository>();
builder.Services.AddScoped<IGreetingRepository, GreetingRepository>();

// Databaseoppsett: Konfigurerer MySQL-tilkobling med retry-logikk
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

// Identity-oppsett: Konfigurerer ASP.NET Identity for brukerautentisering og autorisasjon
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

// Passordpolicy-konfigurasjon: Krever sterke passord
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
});

// Cookie-konfigurasjon: Setter innloggingsstier og session-timeout
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

// Session-konfigurasjon: Konfigurerer distribuert minnehurtiglager og session-innstillinger
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Bruker database-migreringer automatisk ved oppstart
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
        logger.LogError(ex, "En feil oppstod under migrering av databasen.");
    }
}

// Seeder database med innledende data (roller og admin-bruker)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    AuthDbSeeder.SeedAsync(services).GetAwaiter().GetResult();
}

// Sikkerhetshoder: Legger til sikkerhetshoder til alle HTTP-responser
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

// Feilhåndtering: Bruker tilpasset feilside i produksjon, aktiverer HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware-pipeline: Konfigurerer forespørselsbehandlingsrekkefølge
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

// Ruting: Konfigurerer standard MVC-ruting
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
