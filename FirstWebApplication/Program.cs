using FirstWebApplication.DataContext;
using FirstWebApplication.NewFolder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;



// Oppretter en builder for webapplikasjonen
var builder = WebApplication.CreateBuilder(args);

// Legger til MVC-tjenester (Controllers + Views)
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
   options.UseMySql(builder.Configuration.GetConnectionString("DatabaseConnection"),
   new MySqlServerVersion(new Version(11, 8, 3))));

builder.Services.AddDbContext<AuthDbContext>(options =>
   options.UseMySql(builder.Configuration.GetConnectionString("KartVerketAuthDbConnectionString"),
   new MySqlServerVersion(new Version(11, 8, 3))));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
});


var app = builder.Build();

// Konfigurerer HTTP-request pipeline
if (!app.Environment.IsDevelopment())
{
   app.UseExceptionHandler("/Home/Error");
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}

// Tvinger HTTPS
app.UseHttpsRedirection();

// Aktiverer ruting
app.UseRouting();

// Aktiverer autorisering
app.UseAuthentication();
app.UseAuthorization();

// Mapper statiske filer (CSS, JS, bilder osv.) 
app.MapStaticAssets();

// Setter opp standard routing for controllerne
app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();


// Starter applikasjonen
app.Run(); 


