using Attendly.Models;
using Attendly.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Attendly.Services;

public interface IPlannerService
{
 List<ExtendedWeekendViewModel> FindExtendedWeekends(Guid userId, List<Holiday> holidays,
 List<TimetableEntry> timetable, List<Subject> subjects,
 Dictionary<Guid, AttendanceStats> subjectStats, int targetPercentage);
 HolidaySimulatorViewModel SimulateTrip(Guid userId, DateTime startDate, DateTime endDate,
 List<TimetableEntry> timetable, List<Subject> subjects,
 Dictionary<Guid, AttendanceStats> subjectStats, int targetPercentage);
}

public class AttendanceStats
{
 public Guid SubjectId { get; set; }
 public int PresentCount { get; set; }
 public int TotalCount { get; set; }
 public double Percentage { get; set; }
 public int Target { get; set; }
}

public class PlannerService : IPlannerService
{
 private readonly IAttendanceCalculationService _calc;

 public PlannerService(IAttendanceCalculationService calc)
 {
 _calc = calc;
 }

 public List<ExtendedWeekendViewModel> FindExtendedWeekends(Guid userId, List<Holiday> holidays,
 List<TimetableEntry> timetable, List<Subject> subjects,
 Dictionary<Guid, AttendanceStats> subjectStats, int targetPercentage)
 {
 var weekends = new List<ExtendedWeekendViewModel>();

 foreach (var holiday in holidays.OrderBy(h => h.Date))
 {
 var dow = (int)holiday.Date.DayOfWeek;
 DateTime fridayBefore = holiday.Date;
 DateTime mondayAfter = holiday.Date;

 if (dow > 1 && dow <= 5)
 {
 // Find preceding Friday
 fridayBefore = holiday.Date.AddDays(-((int)holiday.Date.DayOfWeek - 5));
 if (fridayBefore.DayOfWeek != DayOfWeek.Friday)
 {
 fridayBefore = holiday.Date.AddDays(-((int)holiday.Date.DayOfWeek + 2) % 7);
 }
 mondayAfter = holiday.Date.AddDays((int)DayOfWeek.Monday - (int)holiday.Date.DayOfWeek);
 if (mondayAfter.DayOfWeek != DayOfWeek.Monday)
 {
 mondayAfter = holiday.Date.AddDays(8 - (int)holiday.Date.DayOfWeek);
 if (mondayAfter.DayOfWeek != DayOfWeek.Monday)
 {
 int days = ((int)DayOfWeek.Monday - (int)holiday.Date.DayOfWeek + 7) % 7;
 if (days == 0) days = 7;
 mondayAfter = holiday.Date.AddDays(days);
 }
 }
 }

 DateTime periodStart = fridayBefore;
 DateTime periodEnd = mondayAfter;

 if (periodEnd < periodStart) continue;

 // Count lectures in the period
 var affectedSubjects = new HashSet<string>();
 int lecturesMissed = 0;
 foreach (var entry in timetable)
 {
 if (IsDateInPeriod(entry.DayOfWeek, periodStart, periodEnd))
 {
 var subj = subjects.FirstOrDefault(s => s.Id == entry.SubjectId);
 if (subj != null)
 {
 affectedSubjects.Add(subj.Name);
 lecturesMissed++;
 }
 }
 }

 if (lecturesMissed == 0) continue;

 // Calculate projected attendance
 double currentOverall = subjectStats.Values.Any()
 ? subjectStats.Values.Average(s => s.Percentage)
 : 100.0;

 int totalPresent = subjectStats.Values.Sum(s => s.PresentCount);
 int totalCount = subjectStats.Values.Sum(s => s.TotalCount);

 int newTotal = totalCount + lecturesMissed;
 int newPresent = totalPresent;

 double projected = _calc.CalculateAttendancePercentage(newPresent, newTotal);
 bool isSafe = projected >= targetPercentage;

 weekends.Add(new ExtendedWeekendViewModel
 {
 HolidayDate = holiday.Date,
 HolidayName = holiday.Name,
 WeekendStart = periodStart,
 WeekendEnd = periodEnd,
 LecturesMissed = lecturesMissed,
 SubjectsAffected = affectedSubjects.ToList(),
 ProjectedAttendance = projected,
 IsSafe = isSafe
 });
 }

 return weekends.OrderBy(w => w.HolidayDate).ToList();
 }

 public HolidaySimulatorViewModel SimulateTrip(Guid userId, DateTime startDate, DateTime endDate,
 List<TimetableEntry> timetable, List<Subject> subjects,
 Dictionary<Guid, AttendanceStats> subjectStats, int targetPercentage)
 {
 int days = (endDate.Date - startDate.Date).Days + 1;
 days = Math.Max(1, days);

 var affectedSubjects = new HashSet<string>();
 int lecturesMissed = 0;

 foreach (var entry in timetable)
 {
 for (int d = 0; d < days; d++)
 {
 var checkDate = startDate.Date.AddDays(d);
 if ((int)checkDate.DayOfWeek == 0 || (int)checkDate.DayOfWeek == 6)
 continue;

 if (IsEntryOnDate(entry, checkDate))
 {
 var subj = subjects.FirstOrDefault(s => s.Id == entry.SubjectId);
 if (subj != null)
 {
 affectedSubjects.Add(subj.Name);
 lecturesMissed++;
 }
 }
 }
 }

 int totalPresent = subjectStats.Values.Sum(s => s.PresentCount);
 int totalCount = subjectStats.Values.Sum(s => s.TotalCount);
 double currentAttendance = _calc.CalculateAttendancePercentage(totalPresent, totalCount);

 int newTotal = totalCount + lecturesMissed;
 double projected = _calc.CalculateAttendancePercentage(totalPresent, newTotal);
 bool isSafe = projected >= targetPercentage;

 return new HolidaySimulatorViewModel
 {
 StartDate = startDate,
 EndDate = endDate,
 DaysAway = days,
 LecturesMissed = lecturesMissed,
 SubjectsAffected = affectedSubjects.ToList(),
 CurrentAttendance = currentAttendance,
 ProjectedAttendance = projected,
 TargetPercentage = targetPercentage,
 IsSafe = isSafe,
 StatusMessage = isSafe
 ? $"Your projected attendance ({projected:F1}%) stays above your target ({targetPercentage}%). This trip is safe."
 : $"WARNING: This trip may push you below your attendance requirement. Projected: {projected:F1}%, Target: {targetPercentage}%."
 };
 }

 private bool IsDateInPeriod(int dayOfWeek, DateTime start, DateTime end)
 {
 // Simple: check if any day in [start, end] matches dayOfWeek
 for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
 {
 if ((int)d.DayOfWeek == dayOfWeek)
 return true;
 }
 return false;
 }

 private bool IsEntryOnDate(TimetableEntry entry, DateTime date)
 {
 return (int)date.DayOfWeek == entry.DayOfWeek;
 }
}
