using Attendly.Data;
using Attendly.Models;
using Attendly.ViewModels;
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
 [ValidateAntiForgeryToken]
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
 IsVerified = true
 };

 await _supabase.CreateProfileAsync(profile);

 _logger.LogInformation("New user registered: {Email}", model.Email);

 return RedirectToAction("Onboarding", new { userId = profile.Id });
 }

 [HttpGet]
 public IActionResult Login()
 {
 if (User.Identity?.IsAuthenticated == true)
 return RedirectToAction("Index", "Dashboard");
 return View(new LoginViewModel());
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Login(LoginViewModel model)
 {
 if (!ModelState.IsValid)
 return View(model);

 var profile = await _supabase.GetProfileByEmailAsync(model.Email);
 if (profile == null || !BCrypt.Net.BCrypt.Verify(model.Password, profile.PasswordHash))
 {
 ModelState.AddModelError(string.Empty, "Invalid email or password.");
 return View(model);
 }

 if (!profile.IsActive)
 {
 ModelState.AddModelError(string.Empty, "Your account has been deactivated.");
 return View(model);
 }

 HttpContext.Session.SetString("UserId", profile.Id.ToString());
 HttpContext.Session.SetString("UserName", profile.FullName ?? "Student");
 HttpContext.Session.SetString("UserEmail", profile.Email);
 HttpContext.Session.SetString("UserRole", profile.Role ?? "student");
 HttpContext.Session.SetString("IsPremium", profile.IsPremium.ToString());

 _logger.LogInformation("User logged in: {Email}", profile.Email);

 return RedirectToAction("Index", "Dashboard");
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Logout()
 {
 HttpContext.Session.Clear();
 _logger.LogInformation("User logged out");
 return RedirectToAction("Login");
 }

 [HttpGet]
 public IActionResult Onboarding(Guid? userId)
 {
 if (userId == null || !User.Identity?.IsAuthenticated == true)
 return RedirectToAction("Register");

 ViewBag.UserId = userId;
 return View();
 }

 [HttpGet]
 public IActionResult ForgotPassword()
 {
 return View();
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
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
}
