using Attendly.Data;
using Attendly.Models;
using Attendly.Services;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("timetable")]
public class TimetableController : Controller
{
 private readonly ISupabaseService _supabase;

 public TimetableController(ISupabaseService supabase)
 {
 _supabase = supabase;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var timetable = await _supabase.GetUserTimetableAsync(userId.Value);
 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 ViewBag.Subjects = subjects.ToList();

 var weekDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
 var grid = new Dictionary<int, List<TimetableEntry>>();

 foreach (var entry in timetable)
 {
 if (!grid.ContainsKey(entry.DayOfWeek))
 grid[entry.DayOfWeek] = new List<TimetableEntry>();
 grid[entry.DayOfWeek].Add(entry);
 }

 foreach (var day in weekDays)
 {
 ViewData[$"day_{day}"] = grid.ContainsKey(Array.IndexOf(weekDays, day) + 1)
 ? grid[Array.IndexOf(weekDays, day) + 1]
 : new List<TimetableEntry>();
 }

 return View();
 }

 [HttpPost("add")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Add(Guid subjectId, int dayOfWeek, string startTime, string endTime, string? room)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
 {
 TempData["Error"] = "Start time and end time are required.";
 return RedirectToAction("Index");
 }

 TimeSpan start = TimeSpan.Parse(startTime);
 TimeSpan end = TimeSpan.Parse(endTime);

 var entry = new TimetableEntry
 {
 UserId = userId.Value,
 SubjectId = subjectId,
 DayOfWeek = dayOfWeek,
 StartTime = start,
 EndTime = end,
 Room = room
 };

 await _supabase.CreateTimetableEntryAsync(entry);
 TempData["Success"] = "Timetable entry added!";
 return RedirectToAction("Index");
 }

 [HttpPost("delete/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Delete(Guid id)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 await _supabase.DeleteTimetableEntryAsync(id, userId.Value);
 TempData["Success"] = "Entry removed.";
 return RedirectToAction("Index");
 }

 private Guid? GetCurrentUserId()
 {
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }
}
