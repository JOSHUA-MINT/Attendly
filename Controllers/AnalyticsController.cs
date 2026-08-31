using Attendly.Data;
using Attendly.Models;
using Attendly.Services;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("analytics")]
public class AnalyticsController : Controller
{
 private readonly ISupabaseService _supabase;
 private readonly IAnalyticsService _analytics;
 private readonly IAttendanceCalculationService _calc;

 public AnalyticsController(ISupabaseService supabase, IAnalyticsService analytics, IAttendanceCalculationService calc)
 {
 _supabase = supabase;
 _analytics = analytics;
 _calc = calc;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 var allRecords = new List<AttendanceRecord>();
 foreach (var subj in subjects)
 {
 allRecords.AddRange(await _supabase.GetAttendanceRecordsAsync(userId.Value, subj.Id));
 }

 var vm = _analytics.BuildAnalytics(userId.Value, subjects, allRecords, new List<TimetableEntry>());
 ViewBag.IsPremium = IsUserPremium();
 return View(vm);
 }

 private Guid? GetCurrentUserId()
 {
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }

 private bool IsUserPremium()
 {
 var exp = HttpContext.Session.GetString("PremiumExpiresAt");
 if (string.IsNullOrEmpty(exp)) return false;
 if (DateTime.TryParse(exp, out var expDate)) return expDate > DateTime.UtcNow;
 return false;
 }
}
