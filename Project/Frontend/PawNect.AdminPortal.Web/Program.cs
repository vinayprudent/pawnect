using Microsoft.AspNetCore.Authentication.Cookies;
using PawNect.AdminPortal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["PawNectApi:BaseUrl"] ?? "https://localhost:7001";
var pathBase = (builder.Configuration["PathBase"] ?? "").Trim().TrimEnd('/');
if (!string.IsNullOrEmpty(pathBase) && !pathBase.StartsWith('/'))
    pathBase = "/" + pathBase;

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IApiClient, ApiClient>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
    c.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = string.IsNullOrEmpty(pathBase) ? "/Account/Login" : pathBase + "/Account/Login";
        options.LogoutPath = string.IsNullOrEmpty(pathBase) ? "/Account/Logout" : pathBase + "/Account/Logout";
        options.AccessDeniedPath = string.IsNullOrEmpty(pathBase) ? "/Account/Login" : pathBase + "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        if (!string.IsNullOrEmpty(pathBase))
            options.Cookie.Path = pathBase;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Path base first so redirects and link generation use /admin
if (!string.IsNullOrEmpty(pathBase))
    app.UsePathBase(pathBase);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(string.IsNullOrEmpty(pathBase) ? "/Home/Error" : pathBase + "/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
