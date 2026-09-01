using Attendly.Data;
using Attendly.Models;
using Attendly.Services;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("attendance")]
public class AttendanceController : Controller
{
 private readonly ISupabaseService _supabase;
 private readonly IAttendanceCalculationService _calc;

 public AttendanceController(ISupabaseService supabase, IAttendanceCalculationService calc)
 {
 _supabase = supabase;
 _calc = calc;
 }

 [HttpGet("")]
public async Task<IActionResult> Index(Guid? subjectId = null)
{
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 ViewBag.Subjects = subjects.ToList();

 Guid targetId = subjectId ?? subjects.FirstOrDefault()?.Id ?? Guid.Empty;
 ViewBag.SelectedSubjectId = targetId;

 if (targetId == Guid.Empty)
 return View(new AttendanceRecordsViewModel { SubjectId = Guid.Empty, SubjectName = "Select a subject" });

 var subject = subjects.FirstOrDefault(s => s.Id == targetId);
 var records = await _supabase.GetAttendanceRecordsAsync(userId.Value, targetId);

 var conducted = records.Count(r => r.Status != "cancelled");
 var present = records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var absent = records.Count(r => r.Status == "absent");
 var cancelled = records.Count(r => r.Status == "cancelled");
 var late = records.Count(r => r.Status == "late");
 var excused = records.Count(r => r.Status == "excused");
 var target = subject?.TargetPercentage ?? 75;

 var pct = _calc.CalculateAttendancePercentage(present, conducted);
 var needed = _calc.CalculateClassesNeededForTarget(present, conducted, target);
 var safe = _calc.CalculateSafeAbsences(present, conducted, target);

 var vm = new AttendanceRecordsViewModel
 {
 SubjectId = targetId,
 SubjectName = subject?.Name ?? "Unknown",
 AttendancePercentage = pct,
 PresentCount = present,
 AbsentCount = absent,
 CancelledCount = cancelled,
 LateCount = late,
 ExcusedCount = excused,
 TargetPercentage = target,
 ClassesNeededForTarget = needed,
 SafeAbsences = safe,
 Records = records.OrderByDescending(r => r.Date).ThenBy(r => r.LectureNumber).Select(r => new AttendanceRecordItem
 {
 Id = r.Id,
 Date = r.Date,
 Status = r.Status,
 Notes = r.Notes
 }).ToList()
 };

 return View(vm);
 }

 [HttpGet("mark")]
 public async Task<IActionResult> Mark(Guid? subjectId = null)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 ViewBag.Subjects = subjects.ToList();

 var timetable = await _supabase.GetUserTimetableAsync(userId.Value);
 var today = DateTime.Now;
 var todayDOW = (int)today.DayOfWeek;
 if (todayDOW == 0) todayDOW = 7;

 var todayEntries = timetable.Where(t => t.DayOfWeek == todayDOW).OrderBy(t => t.StartTime).ToList();

 var todayAttendance = await _supabase.GetAttendanceForDateAsync(userId.Value, today);
 var markModels = new List<AttendanceMarkViewModel>();

 foreach (var entry in todayEntries)
 {
 var subj = subjects.FirstOrDefault(s => s.Id == entry.SubjectId);
 var att = todayAttendance.FirstOrDefault(a => a.SubjectId == entry.SubjectId && a.LectureNumber == 1);
 var canPresent = att == null;
 var canAbsent = att == null;

 markModels.Add(new AttendanceMarkViewModel
 {
 SubjectId = entry.SubjectId,
 SubjectName = subj?.Name ?? "Unknown",
 StartTime = entry.StartTime,
 EndTime = entry.EndTime,
 Room = entry.Room ?? subj?.Room,
 Date = today,
 CanMarkPresent = canPresent,
 CanMarkAbsent = canAbsent,
 MarkedStatus = att?.Status
 });
 }

 if (!markModels.Any())
 {
 ViewBag.NoClassesToday = true;
 }

 return View(markModels);
 }

 [HttpPost("mark/{subjectId}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Mark(Guid subjectId, string status)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 if (string.IsNullOrEmpty(status))
 {
 status = "present";
 }

 var record = new AttendanceRecord
 {
 UserId = userId.Value,
 SubjectId = subjectId,
 Date = DateTime.Now,
 Status = status,
 LectureNumber = 1
 };

 await _supabase.CreateAttendanceAsync(record);
 TempData["Success"] = $"Attendance marked as {status.ToUpper()}!";
 return RedirectToAction("Mark");
 }

 [HttpGet("simulator")]
 public async Task<IActionResult> Simulator()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 ViewBag.Subjects = subjects.ToList();

 Guid targetId = subjects.FirstOrDefault()?.Id ?? Guid.Empty;
 ViewBag.SelectedSubjectId = targetId;

 if (targetId == Guid.Empty)
 return View(new SkipSimulatorViewModel { TargetPercentage = 75 });

 var subject = subjects.First(s => s.Id == targetId);
 var records = await _supabase.GetAttendanceRecordsAsync(userId.Value, targetId);
 var conducted = records.Count(r => r.Status != "cancelled");
 var present = records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");

 var vm = new SkipSimulatorViewModel
 {
 CurrentAttendance = _calc.CalculateAttendancePercentage(present, conducted),
 CurrentTotalLectures = conducted,
 CurrentPresentLectures = present,
 TargetPercentage = subject.TargetPercentage,
 Simulations = _calc.SimulateMultiple(present, conducted, subject.TargetPercentage, 10)
 };

 return View(vm);
 }

 [HttpPost("simulator")]
 public async Task<IActionResult> Simulator(Guid subjectId)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subject = (await _supabase.GetUserSubjectsAsync(userId.Value)).FirstOrDefault(s => s.Id == subjectId);
 if (subject == null) return RedirectToAction("Simulator");

 var records = await _supabase.GetAttendanceRecordsAsync(userId.Value, subjectId);
 var conducted = records.Count(r => r.Status != "cancelled");
 var present = records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");

 ViewBag.Subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 ViewBag.SelectedSubjectId = subjectId;

 var vm = new SkipSimulatorViewModel
 {
 CurrentAttendance = _calc.CalculateAttendancePercentage(present, conducted),
 CurrentTotalLectures = conducted,
 CurrentPresentLectures = present,
 TargetPercentage = subject.TargetPercentage,
 Simulations = _calc.SimulateMultiple(present, conducted, subject.TargetPercentage, 10)
 };

 return View(vm);
 }

 [HttpGet("criteria")]
 public async Task<IActionResult> Criteria()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 ViewBag.Profile = profile;

 return View();
 }

 [HttpPost("criteria")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Criteria(int targetPercentage)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile != null)
 {
 // Store in session for calculations
 HttpContext.Session.SetInt32("TargetPercentage", targetPercentage);
 TempData["Success"] = $"Attendance requirement updated to {targetPercentage}%!";
 }

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
