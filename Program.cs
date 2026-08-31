using Attendly.Configuration;
using Attendly.Data;
using Attendly.Middleware;
using Attendly.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──
builder.Services.AddControllersWithViews(options =>
{
 options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
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
app.UseAuthorization();

// ── Custom Middleware ──
app.UseMiddleware<SessionMiddleware>();

// ── Routes ──
app.MapControllerRoute(
 name: "areas",
 pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

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
 logger.LogWarning(ex, "Supabase initialization failed. App continuing without DB.");
 }

app.Run();
