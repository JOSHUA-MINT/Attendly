using System;
using System.Collections.Generic;

namespace Attendly.ViewModels;

// ── Account ──
public class RegisterViewModel
{
 public string FullName { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 public string ConfirmPassword { get; set; } = string.Empty;
 public string? College { get; set; }
 public string? Course { get; set; }
 public string? Year { get; set; }
 public string? Division { get; set; }
 public string? RailwayLine { get; set; }
 public string? Station { get; set; }
}

public class LoginViewModel
{
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 public bool RememberMe { get; set; }
}

public class ForgotPasswordViewModel
{
 public string Email { get; set; } = string.Empty;
}

// ── Dashboard ──
public class DashboardViewModel
{
 public string UserName { get; set; } = string.Empty;
 public double OverallAttendance { get; set; }
 public int TotalLectures { get; set; }
 public int PresentCount { get; set; }
 public int AbsentCount { get; set; }
 public int TargetPercentage { get; set; }
 public bool IsPremium { get; set; }
 public List<SubjectSummary> SubjectSummaries { get; set; } = new();
 public List<TodaysSchedule> TodaysSchedule { get; set; } = new();
 public List<UpcomingHoliday> UpcomingHolidays { get; set; } = new();
 public List<WeekendOpportunity> WeekendOpportunities { get; set; } = new();
 public int UnreadMessages { get; set; }
 public int PendingRequests { get; set; }
}

public class SubjectSummary
{
 public Guid SubjectId { get; set; }
 public string Name { get; set; } = string.Empty;
 public string? Code { get; set; }
 public double AttendancePercentage { get; set; }
 public int Present { get; set; }
 public int Absent { get; set; }
 public int Total { get; set; }
 public string Status { get; set; } = "SAFE"; // SAFE, WARNING, CRITICAL
 public string? Color { get; set; } = "#3B82F6";
}

public class TodaysSchedule
{
 public Guid SubjectId { get; set; }
 public string SubjectName { get; set; } = string.Empty;
 public TimeSpan StartTime { get; set; }
 public TimeSpan EndTime { get; set; }
 public string? Room { get; set; }
 public bool HasAttendance { get; set; }
 public string? AttendanceStatus { get; set; }
}

public class UpcomingHoliday
{
 public string Name { get; set; } = string.Empty;
 public DateTime Date { get; set; }
 public string Type { get; set; } = "holiday";
 public int DaysUntil { get; set; }
}

public class WeekendOpportunity
{
 public DateTime StartDate { get; set; }
 public DateTime EndDate { get; set; }
 public DateTime WeekendStart => StartDate;
 public DateTime WeekendEnd => EndDate;
 public int LecturesMissed { get; set; }
 public List<string> SubjectsAffected { get; set; } = new();
 public double ProjectedAttendance { get; set; }
 public bool IsSafe { get; set; }
 public string? HolidayName { get; set; }
}

// ── Subjects ──
public class SubjectViewModel
{
 public Guid Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public string? Code { get; set; }
 public string? Professor { get; set; }
 public string? Room { get; set; }
 public int LecturesPerWeek { get; set; }
 public int TargetPercentage { get; set; }
 public string? Color { get; set; }
 public bool IsArchived { get; set; }
 public int TimetableEntriesCount { get; set; }
}

public class SubjectCreateViewModel
{
 public string Name { get; set; } = string.Empty;
 public string? Code { get; set; }
 public string? Professor { get; set; }
 public string? Room { get; set; }
 public int LecturesPerWeek { get; set; } = 3;
 public int TargetPercentage { get; set; } = 75;
 public string? Color { get; set; } = "#3B82F6";
}

// ── Attendance ──
public class AttendanceMarkViewModel
{
 public Guid SubjectId { get; set; }
 public string SubjectName { get; set; } = string.Empty;
 public TimeSpan StartTime { get; set; }
 public TimeSpan EndTime { get; set; }
 public string? Room { get; set; }
 public DateTime Date { get; set; }
 public bool CanMarkPresent { get; set; }
 public bool CanMarkAbsent { get; set; }
 public string? MarkedStatus { get; set; }
}

public class AttendanceRecordsViewModel
{
 public Guid SubjectId { get; set; }
 public string SubjectName { get; set; } = string.Empty;
 public double AttendancePercentage { get; set; }
 public int PresentCount { get; set; }
 public int AbsentCount { get; set; }
 public int CancelledCount { get; set; }
 public int LateCount { get; set; }
 public int ExcusedCount { get; set; }
 public int TargetPercentage { get; set; }
 public int ClassesNeededForTarget { get; set; }
 public int SafeAbsences { get; set; }
 public List<AttendanceRecordItem> Records { get; set; } = new();
}

public class AttendanceRecordItem
{
 public Guid Id { get; set; }
 public DateTime Date { get; set; }
 public string Status { get; set; } = string.Empty;
 public string? Notes { get; set; }
}

// ── Skip Simulator ──
public class SkipSimulatorViewModel
{
 public double CurrentAttendance { get; set; }
 public int CurrentTotalLectures { get; set; }
 public int CurrentPresentLectures { get; set; }
 public int TargetPercentage { get; set; }
 public List<SimulationResult> Simulations { get; set; } = new();
}

public class SimulationResult
{
 public int MissedLectures { get; set; }
 public double ProjectedAttendance { get; set; }
 public bool IsSafe { get; set; }
 public string Message { get; set; } = string.Empty;
}

// ── Analytics ──
public class AnalyticsViewModel
{
 public double OverallAttendance { get; set; }
 public int TargetPercentage { get; set; }
 public List<SubjectAnalytics> SubjectAnalytics { get; set; } = new();
 public List<WeeklyAttendance> WeeklyData { get; set; } = new();
 public List<MonthlyAttendance> MonthlyData { get; set; } = new();
 public AttendanceDistribution Distribution { get; set; } = new();
 public List<Insight> Insights { get; set; } = new();
}

public class SubjectAnalytics
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public double AttendancePercentage { get; set; }
    public int TotalLectures { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public string? Color { get; set; }
    public int TargetPercentage { get; set; } = 75;
}

public class WeeklyAttendance
{
 public string WeekLabel { get; set; } = string.Empty;
 public double AttendancePercentage { get; set; }
}

public class MonthlyAttendance
{
 public string MonthLabel { get; set; } = string.Empty;
 public double AttendancePercentage { get; set; }
}

public class AttendanceDistribution
{
 public int Present { get; set; }
 public int Absent { get; set; }
 public int Cancelled { get; set; }
 public int Late { get; set; }
 public int Excused { get; set; }
}

public class Insight
{
 public string Message { get; set; } = string.Empty;
 public string Type { get; set; } = "info"; // info, warning, success, danger
}

// ── Planner ──
public class HolidaySimulatorViewModel
{
 public DateTime StartDate { get; set; }
 public DateTime EndDate { get; set; }
 public int DaysAway { get; set; }
 public int LecturesMissed { get; set; }
 public List<string> SubjectsAffected { get; set; } = new();
 public double CurrentAttendance { get; set; }
 public double ProjectedAttendance { get; set; }
 public int TargetPercentage { get; set; }
 public bool IsSafe { get; set; }
 public string StatusMessage { get; set; } = string.Empty;
}

public class ExtendedWeekendViewModel
{
 public DateTime HolidayDate { get; set; }
 public string HolidayName { get; set; } = string.Empty;
 public DateTime WeekendStart { get; set; }
 public DateTime WeekendEnd { get; set; }
 public int LecturesMissed { get; set; }
 public List<string> SubjectsAffected { get; set; } = new();
 public double ProjectedAttendance { get; set; }
 public bool IsSafe { get; set; }
}

// ── Connect ──
public class ConnectViewModel
{
 public string SelectedRailwayLine { get; set; } = "western";
 public List<StudentDirectoryViewModel> Students { get; set; } = new();
 public string? SearchQuery { get; set; }
 public string? FilterCollege { get; set; }
 public string? FilterCourse { get; set; }
 public string? FilterYear { get; set; }
 public string? FilterStation { get; set; }
 public List<string> AvailableColleges { get; set; } = new();
 public List<string> AvailableCourses { get; set; } = new();
 public List<string> AvailableYears { get; set; } = new();
 public List<string> AvailableStations { get; set; } = new();
}

public class StudentDirectoryViewModel
{
 public Guid Id { get; set; }
 public string FullName { get; set; } = string.Empty;
 public string? ProfileImageUrl { get; set; }
 public string? College { get; set; }
 public string? Course { get; set; }
 public string? Year { get; set; }
 public string? Station { get; set; }
 public bool IsOnline { get; set; }
 public string? Bio { get; set; }
 public string ConnectionStatus { get; set; } = "none"; // none, pending, accepted
}

// ── Messages ──
public class ConversationViewModel
{
 public Guid ConversationId { get; set; }
 public Guid OtherUserId { get; set; }
 public string OtherUserName { get; set; } = string.Empty;
 public string? OtherUserImage { get; set; }
 public string? LastMessage { get; set; }
 public DateTime? LastMessageAt { get; set; }
 public bool IsOnline { get; set; }
 public int UnreadCount { get; set; }
}

public class ChatViewModel
{
 public Guid ConversationId { get; set; }
 public Guid OtherUserId { get; set; }
 public string OtherUserName { get; set; } = string.Empty;
 public string? OtherUserImage { get; set; }
 public bool IsOnline { get; set; }
 public List<ChatMessageViewModel> Messages { get; set; } = new();
}

public class ChatMessageViewModel
{
 public Guid Id { get; set; }
 public Guid SenderId { get; set; }
 public string Content { get; set; } = string.Empty;
 public DateTime CreatedAt { get; set; }
 public bool IsMine { get; set; }
 public bool IsRead { get; set; }
}

// ── Premium ──
public class PremiumViewModel
{
 public bool IsPremium { get; set; }
 public DateTime? PremiumExpiresAt { get; set; }
 public string RazorpayKeyId { get; set; } = string.Empty;
 public int MonthlyPrice { get; set; } = 49;
 public int YearlyPrice { get; set; } = 399;
 public string? SelectedPlan { get; set; }
}

// ── Profile ──
public class ProfileViewModel
{
 public Guid Id { get; set; }
 public string FullName { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string? College { get; set; }
 public string? Course { get; set; }
 public string? Year { get; set; }
 public string? Division { get; set; }
 public string? RailwayLine { get; set; }
 public string? Station { get; set; }
 public string? Bio { get; set; }
 public string? ProfileImageUrl { get; set; }
 public bool IsPremium { get; set; }
 public DateTime? PremiumExpiresAt { get; set; }
 public int TotalSubjects { get; set; }
 public int TotalConnections { get; set; }
}

// ── Admin ──
public class AdminDashboardViewModel
{
 public int TotalUsers { get; set; }
 public int ActiveUsers { get; set; }
 public int PremiumUsers { get; set; }
 public int TotalReports { get; set; }
 public int PendingReports { get; set; }
 public int TotalConnections { get; set; }
 public int TotalMessages { get; set; }
 public List<ReportViewModel> RecentReports { get; set; } = new();
}

public class ReportViewModel
{
 public Guid Id { get; set; }
 public string ReporterName { get; set; } = string.Empty;
 public string ReportedUserName { get; set; } = string.Empty;
 public string Reason { get; set; } = string.Empty;
 public string Status { get; set; } = string.Empty;
 public DateTime CreatedAt { get; set; }
}
