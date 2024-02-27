using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingReborn.Data;
using SocialBookmarkingReborn.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// PASUL 2 - USERI SI ROLURI
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();


// PASUL 5 -USERI SI ROLURI
// apelam metoda Initialise din SeedData
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//ruta suplimentara fiindca dam un parametru diferit de id pe ruta
app.MapControllerRoute(
    name: "bookmarks",
    pattern: "/{orderByPopular?}",
    defaults: new { controller = "Bookmarks", action = "Index" });

app.MapControllerRoute(
    name: "bookmarksHome",
    pattern: "home/{orderByPopular?}",
    defaults: new { controller = "Bookmarks", action = "Index" });

app.MapControllerRoute(
    name: "bookmarksNew",
    pattern: "bookmarks/new/{local?}",
    defaults: new { controller = "Bookmarks", action = "New" }
);

// ruta pt butoanele de pe show ul userului
app.MapControllerRoute(
    name: "ShowProfile",
    pattern: "Users/show/{id}/{buttonChoice?}",
    defaults: new { controller = "ApplicationUsers", action = "Show" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");
app.MapRazorPages();

app.Run();
