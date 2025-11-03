using FirstWebApplication.DataContext;
using FirstWebApplication.NewFolder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Identity;  vet ikke om den trengs
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
    options.UseMySql(builder.Configuration.GetConnectionString("DatabaseConnection"),
        new MySqlServerVersion(new Version(11, 8, 3))));

// ✅ Legg til Identity + token providers
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders(); // <-- denne er viktig!

//----builder.Services.Configure<IdentityOptions>(options)

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
    
var app = builder.Build();

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
