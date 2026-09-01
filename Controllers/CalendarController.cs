using Attendly.Data;
using Attendly.Models;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("calendar")]
public class CalendarController : Controller
{
 private readonly ISupabaseService _supabase;

 public CalendarController(ISupabaseService supabase)
 {
 _supabase = supabase;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index(int? year, int? month)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var now = DateTime.Now;
 var viewYear = year ?? now.Year;
 var viewMonth = month ?? now.Month;
 var viewDate = new DateTime(viewYear, viewMonth, 1);

 var events = await _supabase.GetUserEventsAsync(userId.Value, viewDate);
 var holidays = await _supabase.GetHolidaysAsync(viewDate, viewDate.AddMonths(1).AddDays(-1));

 // Build calendar days
 var daysInMonth = DateTime.DaysInMonth(viewYear, viewMonth);
 var firstDayOfWeek = (int)viewDate.DayOfWeek;
 if (firstDayOfWeek == 0) firstDayOfWeek = 7;

 ViewBag.ViewDate = viewDate;
 ViewBag.DaysInMonth = daysInMonth;
 ViewBag.FirstDayOfWeek = firstDayOfWeek;
 ViewBag.UserEvents = events;
 ViewBag.Holidays = holidays;

 return View();
 }

 [HttpPost("add-event")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> AddEvent(string title, DateTime date, string eventType, string? description)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var calendarEvent = new CalendarEvent
 {
 UserId = userId.Value,
 Title = title,
 Date = date,
 EventType = eventType,
 Description = description
 };

 await _supabase.CreateEventAsync(calendarEvent);
 TempData["Success"] = "Event added!";
 return RedirectToAction("Index", new { year = date.Year, month = date.Month });
 }

 [HttpPost("delete-event/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteEvent(Guid id)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 await _supabase.DeleteEventAsync(id, userId.Value);
 TempData["Success"] = "Event deleted.";
 return RedirectToAction("Index");
 }

 private Guid? GetCurrentUserId()
 {
 var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
 if (!string.IsNullOrEmpty(claimId) && Guid.TryParse(claimId, out var claimGuid))
 {
 return claimGuid;
 }
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }
}
