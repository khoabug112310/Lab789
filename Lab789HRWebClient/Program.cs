using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// ── Shared DataProtection key ring (SSO) ──────────────────────
var shareKeysPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "ShareKeys"));
Directory.CreateDirectory(shareKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(shareKeysPath))
    .SetApplicationName("Lab789");   // MUST match AuthServer

// ── Cookie Auth – reads the same cookie AuthServer writes ─────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Identity.Application";
    options.DefaultChallengeScheme = "Identity.Application";
    // Do NOT set DefaultSignInScheme here – this app never signs in
}).AddCookie("Identity.Application", options =>
{
    options.Cookie.Name = ".Lab789.Authentication";  // same name as AuthServer
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
    options.SlidingExpiration = true;
    // Redirect unauthenticated users to AuthServer login
    options.Events.OnRedirectToLogin = context =>
    {
        var returnUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
        var loginUrl = "http://localhost:5122/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl);
        context.Response.Redirect(loginUrl);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("http://localhost:5122/Account/AccessDenied");
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
