using Attendly.Data;
using Attendly.Models;
using Attendly.Services;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("planner")]
public class PlannerController : Controller
{
 private readonly ISupabaseService _supabase;
 private readonly IPlannerService _planner;
 private readonly IAttendanceCalculationService _calc;

 public PlannerController(ISupabaseService supabase, IPlannerService planner, IAttendanceCalculationService calc)
 {
 _supabase = supabase;
 _planner = planner;
 _calc = calc;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 var timetable = await _supabase.GetUserTimetableAsync(userId.Value);
 var holidays = await _supabase.GetHolidaysAsync(DateTime.Now, DateTime.Now.AddMonths(4));

 var allRecords = new List<AttendanceRecord>();
 foreach (var subj in subjects)
 {
 allRecords.AddRange(await _supabase.GetAttendanceRecordsAsync(userId.Value, subj.Id));
 }

 var target = subjects.Any() ? subjects.Max(s => s.TargetPercentage) : 75;
 var subjectStats = new Dictionary<Guid, AttendanceStats>();

 foreach (var subj in subjects)
 {
 var records = allRecords.Where(r => r.SubjectId == subj.Id && r.Status != "cancelled").ToList();
 var present = records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var total = records.Count;
 subjectStats[subj.Id] = new AttendanceStats
 {
 SubjectId = subj.Id,
 PresentCount = present,
 TotalCount = total,
 Percentage = _calc.CalculateAttendancePercentage(present, total),
 Target = subj.TargetPercentage
 };
 }

 var weekends = _planner.FindExtendedWeekends(userId.Value, holidays, timetable, subjects, subjectStats, target);
 ViewBag.Subjects = subjects;
 ViewBag.Target = target;

 return View(weekends);
 }

 [HttpGet("simulate")]
 public async Task<IActionResult> Simulate(DateTime? startDate, DateTime? endDate)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 var timetable = await _supabase.GetUserTimetableAsync(userId.Value);
 var holidays = await _supabase.GetHolidaysAsync();

 var allRecords = new List<AttendanceRecord>();
 foreach (var subj in subjects)
 {
 allRecords.AddRange(await _supabase.GetAttendanceRecordsAsync(userId.Value, subj.Id));
 }

 var target = subjects.Any() ? subjects.Max(s => s.TargetPercentage) : 75;
 var subjectStats = new Dictionary<Guid, AttendanceStats>();
 foreach (var subj in subjects)
 {
 var records = allRecords.Where(r => r.SubjectId == subj.Id && r.Status != "cancelled").ToList();
 var present = records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var total = records.Count;
 subjectStats[subj.Id] = new AttendanceStats
 {
 SubjectId = subj.Id,
 PresentCount = present,
 TotalCount = total,
 Percentage = _calc.CalculateAttendancePercentage(present, total),
 Target = subj.TargetPercentage
 };
 }

 HolidaySimulatorViewModel vm;
 if (startDate.HasValue && endDate.HasValue)
 {
 vm = _planner.SimulateTrip(userId.Value, startDate.Value, endDate.Value, timetable, subjects, subjectStats, target);
 }
 else
 {
 vm = new HolidaySimulatorViewModel
 {
 StartDate = DateTime.Now,
 EndDate = DateTime.Now.AddDays(4),
 TargetPercentage = target
 };
 }

 ViewBag.Target = target;
 return View(vm);
 }

 [HttpPost("simulate")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Simulate(DateTime startDate, DateTime endDate)
 {
 return await Simulate((DateTime?)startDate, (DateTime?)endDate);
 }

 private Guid? GetCurrentUserId()
 {
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }
}
