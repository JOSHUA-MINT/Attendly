using Attendly.Data;
using Attendly.Models;
using Attendly.Services;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Attendly.Controllers;

[Route("dashboard")]
public class DashboardController : Controller
{
 private readonly ISupabaseService _supabase;
 private readonly IAttendanceCalculationService _calc;
 private readonly IPlannerService _planner;

 public DashboardController(ISupabaseService supabase, IAttendanceCalculationService calc, IPlannerService planner)
 {
 _supabase = supabase;
 _calc = calc;
 _planner = planner;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile == null) return RedirectToAction("Login", "Account");

 // Load all user data
 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 var allRecords = new List<AttendanceRecord>();
 foreach (var subj in subjects)
 {
 var records = await _supabase.GetAttendanceRecordsAsync(userId.Value, subj.Id);
 allRecords.AddRange(records);
 }
 var timetable = await _supabase.GetUserTimetableAsync(userId.Value);
 var todayAttendance = await _supabase.GetAttendanceForDateAsync(userId.Value, DateTime.Now);

 // ── Overall Stats ──
 var conducted = allRecords.Count(r => r.Status != "cancelled");
 var present = allRecords.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var absent = allRecords.Count(r => r.Status == "absent");
 var target = subjects.Any() ? subjects.Max(s => s.TargetPercentage) : 75;
 var overallPct = _calc.CalculateAttendancePercentage(present, conducted);

 // ── Subject Summaries ──
 var summaries = subjects.Select(s =>
 {
 var subjRecords = allRecords.Where(r => r.SubjectId == s.Id && r.Status != "cancelled").ToList();
 var sp = subjRecords.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var st = subjRecords.Count;
 var pct = _calc.CalculateAttendancePercentage(sp, st);
 var status = _calc.GetAttendanceStatus(pct, s.TargetPercentage);

 return new SubjectSummary
 {
 SubjectId = s.Id,
 Name = s.Name,
 Code = s.Code,
 AttendancePercentage = pct,
 Present = sp,
 Absent = st - sp,
 Total = st,
 Status = status,
 Color = s.Color
 };
 }).OrderByDescending(s => s.AttendancePercentage).ToList();

 // ── Today's Schedule ──
 var todayDOW = (int)DateTime.Now.DayOfWeek;
 if (todayDOW == 0) todayDOW = 7;
 var todayEntries = timetable.Where(t => t.DayOfWeek == todayDOW).OrderBy(t => t.StartTime).ToList();

 var todaySchedule = todayEntries.Select(entry =>
 {
 var subj = subjects.FirstOrDefault(s => s.Id == entry.SubjectId);
 var att = todayAttendance.FirstOrDefault(a => a.SubjectId == entry.SubjectId);
 return new TodaysSchedule
 {
 SubjectId = entry.SubjectId,
 SubjectName = subj?.Name ?? "Unknown",
 StartTime = entry.StartTime,
 EndTime = entry.EndTime,
 Room = entry.Room ?? subj?.Room,
 HasAttendance = att != null,
 AttendanceStatus = att?.Status
 };
 }).ToList();

 // ── Upcoming Holidays ──
 var holidays = await _supabase.GetHolidaysAsync(DateTime.Now, DateTime.Now.AddMonths(3));
 var upcomingHolidays = holidays.Select(h => new UpcomingHoliday
 {
 Name = h.Name,
 Date = h.Date,
 Type = h.Type,
 DaysUntil = (h.Date.Date - DateTime.Now.Date).Days
 }).Take(5).ToList();

        // ── Weekend Opportunities ──
        var subjectStats = new Dictionary<Guid, AttendanceStats>();
        foreach (var subj in subjects)
        {
            var records = allRecords.Where(r => r.SubjectId == subj.Id && r.Status != "cancelled").ToList();
            subjectStats[subj.Id] = new AttendanceStats
            {
                SubjectId = subj.Id,
                PresentCount = records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused"),
                TotalCount = records.Count,
                Percentage = _calc.CalculateAttendancePercentage(
                    records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused"),
                    records.Count),
                Target = subj.TargetPercentage
            };
        }

        var weekendOpps = _planner.FindExtendedWeekends(userId.Value, holidays, timetable, subjects, subjectStats, target);

        // ── Messages & Notifications ──
        var conversations = await _supabase.GetUserConversationsAsync(userId.Value);
        int unread = 0;
        foreach (var conv in conversations)
        {
            unread += (await _supabase.GetMessagesAsync(conv.Id)).Count(m => m.ReadAt == null && m.SenderId != userId);
        }

        var pendingRequests = (await _supabase.GetUserConnectionsAsync(userId.Value))
            .Count(c => c.Status == "pending" && c.ReceiverId == userId);

        var vm = new DashboardViewModel
        {
            UserName = profile.FullName ?? "Student",
            OverallAttendance = overallPct,
            TotalLectures = conducted,
            PresentCount = present,
            AbsentCount = absent,
            TargetPercentage = target,
            IsPremium = profile.IsPremium && profile.PremiumExpiresAt > DateTime.UtcNow,
            SubjectSummaries = summaries,
            TodaysSchedule = todaySchedule,
            UpcomingHolidays = upcomingHolidays,
            WeekendOpportunities = weekendOpps.Select(o => new WeekendOpportunity
            {
                StartDate = o.WeekendStart,
                EndDate = o.WeekendEnd,
                LecturesMissed = o.LecturesMissed,
                SubjectsAffected = o.SubjectsAffected,
                ProjectedAttendance = o.ProjectedAttendance,
                IsSafe = o.IsSafe,
                HolidayName = o.HolidayName
            }).ToList(),
            UnreadMessages = unread,
            PendingRequests = pendingRequests
        };

 ViewBag.IsPremium = profile.IsPremium && profile.PremiumExpiresAt > DateTime.UtcNow;
 return View(vm);
 }

 private Guid? GetCurrentUserId()
 {
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }
}
