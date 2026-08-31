using Attendly.Models;
using Attendly.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Attendly.Services;

public interface IAnalyticsService
{
 AnalyticsViewModel BuildAnalytics(Guid userId, List<Subject> subjects,
 List<AttendanceRecord> allRecords, List<TimetableEntry> timetable);
 double CalculateWeeklyAttendance(List<AttendanceRecord> records);
 double CalculateMonthlyAttendance(List<AttendanceRecord> records, int year, int month);
 List<Insight> GenerateInsights(AnalyticsViewModel analytics);
}

public class AnalyticsService : IAnalyticsService
{
 private readonly IAttendanceCalculationService _calc;

 public AnalyticsService(IAttendanceCalculationService calc)
 {
 _calc = calc;
 }

 public AnalyticsViewModel BuildAnalytics(Guid userId, List<Subject> subjects,
 List<AttendanceRecord> allRecords, List<TimetableEntry> timetable)
 {
 var presentTotal = allRecords.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var absentTotal = allRecords.Count(r => r.Status == "absent");
 var totalCount = allRecords.Count(r => r.Status != "cancelled");
 var target = subjects.Any() ? subjects.Max(s => s.TargetPercentage) : 75;

 var overallAttendance = _calc.CalculateAttendancePercentage(presentTotal, totalCount);

 var subjectAnalytics = subjects.Select(s =>
 {
 var records = allRecords.Where(r => r.SubjectId == s.Id && r.Status != "cancelled").ToList();
 var present = records.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var absent = records.Count(r => r.Status == "absent");
 var total = records.Count;
 var pct = _calc.CalculateAttendancePercentage(present, total);

 return new SubjectAnalytics
 {
 SubjectId = s.Id,
 SubjectName = s.Name,
 AttendancePercentage = pct,
 TotalLectures = total,
 Present = present,
 Absent = absent,
 Color = s.Color,
 TargetPercentage = s.TargetPercentage
 };
 }).ToList();

 var weeklyData = CalculateWeeklyTrends(allRecords);
 var monthlyData = CalculateMonthlyTrends(allRecords);
 var distribution = new AttendanceDistribution
 {
 Present = presentTotal,
 Absent = absentTotal,
 Cancelled = allRecords.Count(r => r.Status == "cancelled"),
 Late = allRecords.Count(r => r.Status == "late"),
 Excused = allRecords.Count(r => r.Status == "excused")
 };

 var analytics = new AnalyticsViewModel
 {
 OverallAttendance = overallAttendance,
 TargetPercentage = target,
 SubjectAnalytics = subjectAnalytics,
 WeeklyData = weeklyData,
 MonthlyData = monthlyData,
 Distribution = distribution,
 Insights = GenerateInsights(new AnalyticsViewModel
 {
 OverallAttendance = overallAttendance,
 SubjectAnalytics = subjectAnalytics,
 TargetPercentage = target
 })
 };

 return analytics;
 }

 public List<WeeklyAttendance> CalculateWeeklyTrends(List<AttendanceRecord> records)
 {
 var grouped = records
 .Where(r => r.Status != "cancelled")
 .GroupBy(r => {
 var weekStart = r.Date.Date.AddDays(-(int)r.Date.DayOfWeek);
 return weekStart;
 })
 .OrderBy(g => g.Key)
 .Take(12)
 .Select(g => {
 var present = g.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var total = g.Count();
 var pct = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;
 return new WeeklyAttendance
 {
 WeekLabel = g.Key.ToString("MMM dd"),
 AttendancePercentage = pct
 };
 }).ToList();

 return grouped;
 }

 public double CalculateWeeklyAttendance(List<AttendanceRecord> records)
 {
 var valid = records.Where(r => r.Status != "cancelled").ToList();
 if (!valid.Any()) return 0;
 var present = valid.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 return Math.Round((double)present / valid.Count * 100, 1);
 }

 public double CalculateMonthlyAttendance(List<AttendanceRecord> records, int year, int month)
 {
 var valid = records.Where(r => r.Status != "cancelled" && r.Date.Year == year && r.Date.Month == month).ToList();
 if (!valid.Any()) return 0;
 var present = valid.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 return Math.Round((double)present / valid.Count * 100, 1);
 }

 public List<MonthlyAttendance> CalculateMonthlyTrends(List<AttendanceRecord> records)
 {
 var grouped = records
 .Where(r => r.Status != "cancelled")
 .GroupBy(r => new { r.Date.Year, r.Date.Month })
 .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
 .Take(12)
 .Select(g => {
 var present = g.Count(r => r.Status == "present" || r.Status == "late" || r.Status == "excused");
 var total = g.Count();
 var pct = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;
 return new MonthlyAttendance
 {
 MonthLabel = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
 AttendancePercentage = pct
 };
 }).ToList();

 return grouped;
 }

 public List<Insight> GenerateInsights(AnalyticsViewModel analytics)
 {
 var insights = new List<Insight>();

 if (analytics.OverallAttendance >= analytics.TargetPercentage + 5)
 {
 insights.Add(new Insight
 {
 Message = $"Your overall attendance is {analytics.OverallAttendance:F1}%, which is {analytics.OverallAttendance - analytics.TargetPercentage:F1}% above your target. Great job!",
 Type = "success"
 });
 }
 else if (analytics.OverallAttendance < analytics.TargetPercentage - 5)
 {
 insights.Add(new Insight
 {
 Message = $"Your overall attendance ({analytics.OverallAttendance:F1}%) is below your target ({analytics.TargetPercentage}%). Time to focus on attendance.",
 Type = "danger"
 });
 }
 else if (analytics.OverallAttendance >= analytics.TargetPercentage)
 {
 insights.Add(new Insight
 {
 Message = $"You're meeting your attendance target. Keep it up!",
 Type = "info"
 });
 }

 if (analytics.SubjectAnalytics.Any())
 {
 var lowest = analytics.SubjectAnalytics.OrderBy(s => s.AttendancePercentage).First();
 insights.Add(new Insight
 {
 Message = $"{lowest.SubjectName} is your lowest-attendance subject at {lowest.AttendancePercentage:F1}%.",
 Type = "warning"
 });

 var highest = analytics.SubjectAnalytics.OrderByDescending(s => s.AttendancePercentage).First();
 insights.Add(new Insight
 {
 Message = $"{highest.SubjectName} has your best attendance at {highest.AttendancePercentage:F1}%.",
 Type = "success"
 });

 var safeSubjects = analytics.SubjectAnalytics
 .Where(s => s.TotalLectures > 0)
 .Select(s => _calc.CalculateSafeAbsences(s.Present, s.TotalLectures, s.TargetPercentage))
 .Where(x => x >= 1)
 .ToList();

 if (safeSubjects.Any())
 {
 insights.Add(new Insight
 {
 Message = $"You have safe absences available in {safeSubjects.Count} subject(s).",
 Type = "info"
 });
 }
 }

 return insights;
 }
}
