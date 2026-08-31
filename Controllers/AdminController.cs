using Attendly.Data;
using Attendly.Models;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("admin")]
public class AdminController : Controller
{
 private readonly ISupabaseService _supabase;

 public AdminController(ISupabaseService supabase)
 {
 _supabase = supabase;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile?.Role != "admin")
 return RedirectToAction("Index", "Dashboard");

 var students = await _supabase.GetAllStudentsAsync();
 var reports = await _supabase.GetReportsAsync("pending");

  var recentReports = new List<ReportViewModel>();
  foreach (var r in reports)
  {
      var reporter = await _supabase.GetProfileByIdAsync(r.ReporterId);
      var reportedUser = await _supabase.GetProfileByIdAsync(r.ReportedUserId);
      recentReports.Add(new ReportViewModel
      {
          Id = r.Id,
          ReporterName = reporter?.FullName ?? "Unknown",
          ReportedUserName = reportedUser?.FullName ?? "Unknown",
          Reason = r.Reason,
          Status = r.Status,
          CreatedAt = r.CreatedAt
      });
  }

  var vm = new AdminDashboardViewModel
  {
      TotalUsers = students.Count,
      ActiveUsers = students.Count(s => s.IsActive),
      PremiumUsers = students.Count(s => s.IsPremium),
      TotalReports = (await _supabase.GetReportsAsync()).Count,
      PendingReports = reports.Count,
      TotalConnections = (await _supabase.GetUserConnectionsAsync(userId.Value)).Count,
      RecentReports = recentReports
  };

 return View(vm);
 }

 [HttpGet("holidays")]
 public async Task<IActionResult> ManageHolidays()
 {
 var holidays = await _supabase.GetHolidaysAsync();
 return View(holidays);
 }

 [HttpPost("holidays/add")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> AddHoliday(string name, DateTime date, string type, string? region)
 {
 var holiday = new Holiday
 {
 Name = name,
 Date = date,
 Type = type ?? "public",
 Region = region
 };

 await _supabase.CreateHolidayAsync(holiday);
 TempData["Success"] = "Holiday added!";
 return RedirectToAction("ManageHolidays");
 }

 [HttpPost("holidays/delete/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteHoliday(Guid id)
 {
 await _supabase.DeleteHolidayAsync(id);
 TempData["Success"] = "Holiday removed.";
 return RedirectToAction("ManageHolidays");
 }

 [HttpGet("reports")]
 public async Task<IActionResult> ManageReports(string? status = null)
 {
 var reports = await _supabase.GetReportsAsync(status);
 return View(reports);
 }

 [HttpPost("reports/{id}/action")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> ReviewReport(Guid id, string action)
 {
 var report = (await _supabase.GetReportsAsync()).FirstOrDefault(r => r.Id == id);
 if (report != null)
 {
 report.Status = action == "dismiss" ? "dismissed" : "actioned";
 report.ReviewedAt = DateTime.UtcNow;
 await _supabase.UpdateReportAsync(report);
 TempData["Success"] = action == "dismiss" ? "Report dismissed." : "Action taken on report.";
 }

 return RedirectToAction("ManageReports");
 }

 private Guid? GetCurrentUserId()
 {
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }
}
