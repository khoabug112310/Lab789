using AuthServer.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthServer.Services;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────
// 1. Database
// ──────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ──────────────────────────────────────────────────────────────
// 2. DataProtection – shared key ring for SSO cookie
// ──────────────────────────────────────────────────────────────
var shareKeysPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "ShareKeys"));
Directory.CreateDirectory(shareKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(shareKeysPath))
    .SetApplicationName("Lab789");            // ← must match all clients

// ──────────────────────────────────────────────────────────────
// 3. Identity (for user/password management + SignInManager)
// ──────────────────────────────────────────────────────────────
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ──────────────────────────────────────────────────────────────
// 4. Cookie – MUST use scheme "Identity.Application" so that
//    HR/Sales clients (which call AddCookie("Identity.Application"))
//    can decrypt the same ticket via the shared DataProtection key.
// ──────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".Lab789.Authentication";   // same name on all apps
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // works on HTTP
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
    options.Cookie.Domain = null;               // localhost – no domain needed
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ──────────────────────────────────────────────────────────────
// 5. Pipeline
// ──────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
// No HTTPS redirect – runs on HTTP so cookie is not Secure-flagged

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed data
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitAsync(scope.ServiceProvider);
}

app.Run();
