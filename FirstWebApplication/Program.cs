using FirstWebApplication.DataContext;
using FirstWebApplication.NewFolder;
using FirstWebApplication.Repository;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

// Oppretter en builder for webapplikasjonen
var builder = WebApplication.CreateBuilder(args);

// Legger til MVC-tjenester (Controllers + Views)
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();
builder.Services.AddScoped<IObstacleRepository, ObstacleRepository>();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DatabaseConnection"),
    new MySqlServerVersion(new Version(11, 8, 3))));

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
