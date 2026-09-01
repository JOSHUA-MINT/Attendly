using Attendly.Data;
using Attendly.Models;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace Attendly.Controllers;

public class AccountController : Controller
{
 private readonly ISupabaseService _supabase;
 private readonly ILogger<AccountController> _logger;

 public AccountController(ISupabaseService supabase, ILogger<AccountController> logger)
 {
 _supabase = supabase;
 _logger = logger;
 }

 [HttpGet]
 public IActionResult Register()
 {
 return View(new RegisterViewModel());
 }

 [HttpPost]
 public async Task<IActionResult> Register(RegisterViewModel model)
 {
 if (!ModelState.IsValid)
 return View(model);

 var existing = await _supabase.GetProfileByEmailAsync(model.Email);
 if (existing != null)
 {
 ModelState.AddModelError("Email", "An account with this email already exists.");
 return View(model);
 }

 var profile = new StudentProfile
 {
 Email = model.Email,
 PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
 FullName = model.FullName,
 College = model.College,
 Course = model.Course,
 Year = model.Year,
 Division = model.Division,
 RailwayLine = model.RailwayLine,
 Station = model.Station,
 IsVerified = true,
 CreatedAt = DateTime.UtcNow
 };

        var created = await _supabase.CreateProfileAsync(profile);

        _logger.LogInformation("New user registered: {Email}, Id={Id}", model.Email, created?.Id ?? profile.Id);

        // Auto-login after registration
        await SignInUser(created ?? profile);

        TempData["Success"] = "Welcome to Attendly! Let's get you started.";
        return RedirectToAction("Index", "Dashboard");
 }

 [HttpGet]
 public IActionResult Login()
 {
 return View(new LoginViewModel());
 }

 [HttpPost]
 public async Task<IActionResult> Login(LoginViewModel model)
 {
 _logger.LogInformation("Login POST hit: Email={Email}, ModelStateValid={Valid}",
 model.Email, ModelState.IsValid);

 if (!ModelState.IsValid)
 {
 _logger.LogWarning("Login ModelState invalid: {Errors}",
 string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
 return View(model);
 }

 var profile = await _supabase.GetProfileByEmailAsync(model.Email);
 if (profile == null || !BCrypt.Net.BCrypt.Verify(model.Password, profile.PasswordHash))
 {
 _logger.LogWarning("Login failed: email={Email}, profileFound={Found}", model.Email, profile != null);
 ModelState.AddModelError(string.Empty, "Invalid email or password.");
 return View(model);
 }

 if (!profile.IsActive)
 {
 _logger.LogWarning("Login blocked: account deactivated, email={Email}", model.Email);
 ModelState.AddModelError(string.Empty, "Your account has been deactivated.");
 return View(model);
 }

 _logger.LogInformation("Login success: signing in user {Email}, Role={Role}", profile.Email, profile.Role);
 await SignInUser(profile);

 _logger.LogInformation("Login redirecting to Dashboard for {Email}", profile.Email);
 return RedirectToAction("Index", "Dashboard");
 }

 [HttpGet]
 [HttpPost]
 public async Task<IActionResult> Logout()
 {
 await HttpContext.SignOutAsync();
 HttpContext.Session.Clear();
 _logger.LogInformation("User logged out");
 return RedirectToAction("Login");
 }

 [HttpGet]
 public IActionResult Onboarding(Guid? userId)
 {
 return View();
 }

 [HttpGet]
 public IActionResult ForgotPassword()
 {
 return View();
 }

 [HttpPost]
 public async Task<IActionResult> ForgotPassword(string email)
 {
 var profile = await _supabase.GetProfileByEmailAsync(email);
 if (profile != null)
 {
 TempData["Info"] = "If an account exists with this email, you'll receive password reset instructions.";
 }
 else
 {
 TempData["Info"] = "If an account exists with this email, you'll receive password reset instructions.";
 }
 return RedirectToAction("Login");
 }

 // ── Helper: create auth cookie from profile ──
 private async Task SignInUser(StudentProfile profile)
 {
 var userId = profile.Id.ToString();
 HttpContext.Session.SetString("UserId", userId);
 HttpContext.Session.SetString("UserName", profile.FullName ?? "Student");
 HttpContext.Session.SetString("UserEmail", profile.Email);
 HttpContext.Session.SetString("UserRole", profile.Role ?? "student");
 HttpContext.Session.SetString("IsPremium", profile.IsPremium.ToString());

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, profile.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, profile.FullName ?? "Student"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, profile.Role ?? "student"),
            new System.Security.Claims.Claim("UserEmail", profile.Email),
            new System.Security.Claims.Claim("UserRole", profile.Role ?? "student"),
            new System.Security.Claims.Claim("IsPremium", profile.IsPremium.ToString())
        };

 var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
 var principal = new System.Security.Claims.ClaimsPrincipal(identity);

 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
 _logger.LogInformation("SignInUser: UserId={UserId}, SessionId={SessionId}",
 userId, HttpContext.Session.Id ?? "null");
 }
}
