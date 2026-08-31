using Attendly.Configuration;
using Attendly.Data;
using Attendly.Middleware;
using Attendly.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──
builder.Services.AddControllersWithViews(options =>
{
 options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ── Authentication (Cookie-based, session-backed) ──
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
 .AddCookie(options =>
 {
 options.LoginPath = "/Account/Login";
 options.LogoutPath = "/Account/Logout";
 options.AccessDeniedPath = "/Account/Login";
 options.ExpireTimeSpan = TimeSpan.FromHours(8);
 options.SlidingExpiration = true;
 options.Cookie.HttpOnly = true;
 options.Cookie.IsEssential = true;
 });

// ── HttpClients ──
builder.Services.AddHttpClient("Razorpay", client =>
{
 client.BaseAddress = new Uri("https://api.razorpay.com/v1/");
 client.DefaultRequestHeaders.Add("User-Agent", "Attendly/1.0");
 });

// ── Session ──
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
 options.IdleTimeout = TimeSpan.FromHours(8);
 options.Cookie.HttpOnly = true;
 options.Cookie.IsEssential = true;
 options.Cookie.SameSite = SameSiteMode.Lax;
 });

// ── Services ──
builder.Services.AddScoped<ISupabaseService, SupabaseService>();
builder.Services.AddScoped<IAttendanceCalculationService, AttendanceCalculationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPlannerService, PlannerService>();
builder.Services.AddScoped<IRazorpayService, RazorpayService>();

// ── Load Config ──
SupabaseConfig.Url = builder.Configuration["Supabase:Url"] ?? "";
SupabaseConfig.AnonKey = builder.Configuration["Supabase:AnonKey"] ?? "";
SupabaseConfig.ServiceRoleKey = builder.Configuration["Supabase:ServiceRoleKey"] ?? "";

RazorpayConfig.KeyId = builder.Configuration["Razorpay:KeyId"] ?? "";
RazorpayConfig.KeySecret = builder.Configuration["Razorpay:KeySecret"] ?? "";

AppConfig.SiteName = builder.Configuration["AppSettings:SiteName"] ?? "Attendly";
AppConfig.SupportEmail = builder.Configuration["AppSettings:SupportEmail"] ?? "support@attendly.app";

var app = builder.Build();

// ── Middleware Pipeline ──
if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
 app.UseHsts();
 }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

// ── Custom Middleware: sync User principal from session ──
app.UseMiddleware<SessionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// ── Routes ──
app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Initialize Supabase ──
try
{
 using (var scope = app.Services.CreateScope())
 {
 var supabase = scope.ServiceProvider.GetRequiredService<ISupabaseService>();
 await supabase.InitializeAsync();
 }
}
catch (Exception ex)
{
 var logger = app.Logger;
 logger.LogWarning(ex, "Supabase init failed. App continuing without DB.");
 }

app.Run();
