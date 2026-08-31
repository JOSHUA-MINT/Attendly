# Graph Report - Attendly  (2026-09-01)

## Corpus Check
- Corpus is ~24,264 words - fits in a single context window. You may not need a graph.

## Summary
- 888 nodes · 1737 edges · 71 communities (46 shown, 25 thin omitted)
- Extraction: 88% EXTRACTED · 12% INFERRED · 0% AMBIGUOUS · INFERRED: 206 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Dashboard Controller
- Supabase Data Service
- Analytics Controller
- Supabase Interface
- Premium & Razorpay
- Admin Controller
- Attendance Controller
- Subjects Controller
- Cross-Controller Refs
- Account Controller
- Connect Controller
- Profile Controller
- Calendar Controller
- Student Profile Model
- Main ViewModels
- Subject Model
- Attendance ViewModels
- Planner ViewModels
- Dashboard ViewModels
- Conversation & BaseModel
- Connect ViewModels
- App Configuration
- Message Model
- Home Controller
- Timetable Controller
- Premium ViewModels
- Admin ViewModels
- Attendance Mark ViewModels
- Messages ViewModels
- Planner Sim ViewModels
- Connect Directory ViewModels
- Subjects ViewModels
- Calendar ViewModels
- Attendance VMs + DateTime
- Project & Dependencies
- ViewImports & Namespaces
- Admin Dashboard Views
- Analytics Views
- Attendance Views
- Messages Chat Views
- Dashboard Views
- Planner Sim Views
- Login Views
- Premium Views
- Register Views
- Skip Simulator Views
- Subject Create Views
- Connect Views
- Holiday Views
- Reports Views
- Mark Attendance Views
- Messages Index Views
- Planner Views
- Student Directory Views
- Subjects Index Views
- Setup Script
- Profile Edit Views
- Profile Views
- Shared Layout
- Shared Footer
- Shared Navbar

## God Nodes (most connected - your core abstractions)
1. `ISupabaseService` - 63 edges
2. `SupabaseService` - 55 edges
3. `StudentProfile` - 53 edges
4. `AttendanceRecord` - 36 edges
5. `TimetableEntry` - 34 edges
6. `Report` - 26 edges
7. `Attendly.Models` - 25 edges
8. `Connection` - 25 edges
9. `Subscription` - 24 edges
10. `Subject` - 23 edges

## Surprising Connections (you probably didn't know these)
- `AccountController` --references--> `ISupabaseService`  [EXTRACTED]
  Controllers/AccountController.cs → Data/ISupabaseService.cs
- `AdminController` --references--> `ISupabaseService`  [EXTRACTED]
  Controllers/AdminController.cs → Data/ISupabaseService.cs
- `AnalyticsController` --references--> `ISupabaseService`  [EXTRACTED]
  Controllers/AnalyticsController.cs → Data/ISupabaseService.cs
- `AnalyticsController` --references--> `IAttendanceCalculationService`  [EXTRACTED]
  Controllers/AnalyticsController.cs → Services/AttendanceCalculationService.cs
- `AttendanceController` --references--> `ISupabaseService`  [EXTRACTED]
  Controllers/AttendanceController.cs → Data/ISupabaseService.cs

## Import Cycles
- None detected.

## Communities (71 total, 25 thin omitted)

### Community 0 - "Dashboard Controller"
Cohesion: 0.07
Nodes (26): Controller, DashboardController, Guid, HttpGet, IActionResult, Task, MessagesController, Guid (+18 more)

### Community 1 - "Supabase Data Service"
Cohesion: 0.07
Nodes (22): Client, SupabaseService, Conversation, DateTime, Guid, List, Subject, Task (+14 more)

### Community 2 - "Analytics Controller"
Cohesion: 0.06
Nodes (44): AnalyticsController, Guid, HttpGet, IActionResult, Task, AttendanceRecord, CreatedAt, Date (+36 more)

### Community 3 - "Supabase Interface"
Cohesion: 0.06
Nodes (39): Dictionary, Subject, TimetableEntry, CreatedAt, DayOfWeek, EndTime, Id, Room (+31 more)

### Community 4 - "Premium & Razorpay"
Cohesion: 0.07
Nodes (33): PremiumController, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken, HttpClient (+25 more)

### Community 5 - "Admin Controller"
Cohesion: 0.09
Nodes (25): AdminController, DateTime, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken (+17 more)

### Community 6 - "Attendance Controller"
Cohesion: 0.12
Nodes (17): AttendanceController, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken, isSafe (+9 more)

### Community 7 - "Subjects Controller"
Cohesion: 0.11
Nodes (26): SubjectsController, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken, SubjectCreateViewModel (+18 more)

### Community 8 - "Cross-Controller Refs"
Cohesion: 0.17
Nodes (11): Attendly.Configuration, Attendly.Services, Attendly.Middleware, Attendly.ViewModels, Attendly.Controllers, Attendly.Data, Attendly.Models, HttpContext (+3 more)

### Community 9 - "Account Controller"
Cohesion: 0.11
Nodes (23): AccountController, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken, ILogger (+15 more)

### Community 10 - "Connect Controller"
Cohesion: 0.13
Nodes (20): ConnectController, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken, Connection (+12 more)

### Community 11 - "Profile Controller"
Cohesion: 0.11
Nodes (23): ProfileController, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken, ProfileViewModel (+15 more)

### Community 12 - "Calendar Controller"
Cohesion: 0.11
Nodes (20): CalendarController, DateTime, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken (+12 more)

### Community 13 - "Student Profile Model"
Cohesion: 0.08
Nodes (25): StudentProfile, Bio, College, Course, CreatedAt, Division, Email, FullName (+17 more)

### Community 14 - "Main ViewModels"
Cohesion: 0.09
Nodes (23): ConnectViewModel, AvailableColleges, AvailableCourses, AvailableStations, AvailableYears, FilterCollege, FilterCourse, FilterStation (+15 more)

### Community 15 - "Subject Model"
Cohesion: 0.10
Nodes (19): Subject, AttendanceRecords, Code, Color, CreatedAt, Id, IsArchived, LecturesPerWeek (+11 more)

### Community 16 - "Attendance ViewModels"
Cohesion: 0.11
Nodes (19): AttendanceRecordItem, Date, Id, Notes, Status, AttendanceRecordsViewModel, AbsentCount, AttendancePercentage (+11 more)

### Community 17 - "Planner ViewModels"
Cohesion: 0.12
Nodes (16): ChatMessageViewModel, Content, CreatedAt, Id, IsMine, IsRead, SenderId, ChatViewModel (+8 more)

### Community 18 - "Dashboard ViewModels"
Cohesion: 0.12
Nodes (16): AdminDashboardViewModel, ActiveUsers, PendingReports, PremiumUsers, RecentReports, TotalConnections, TotalMessages, TotalReports (+8 more)

### Community 19 - "Conversation & BaseModel"
Cohesion: 0.14
Nodes (13): BaseModel, Conversation, CreatedAt, Id, LastMessageAt, Messages, User1, User1Id (+5 more)

### Community 20 - "Connect ViewModels"
Cohesion: 0.14
Nodes (14): DashboardViewModel, AbsentCount, IsPremium, OverallAttendance, PendingRequests, PresentCount, SubjectSummaries, TargetPercentage (+6 more)

### Community 21 - "App Configuration"
Cohesion: 0.15
Nodes (12): AppConfig, SiteName, SupportEmail, RazorpayConfig, IsConfigured, KeyId, KeySecret, SupabaseConfig (+4 more)

### Community 22 - "Message Model"
Cohesion: 0.15
Nodes (12): Message, Content, Conversation, ConversationId, CreatedAt, Id, IsDeleted, ReadAt (+4 more)

### Community 23 - "Home Controller"
Cohesion: 0.40
Nodes (3): HomeController, HttpGet, IActionResult

### Community 24 - "Timetable Controller"
Cohesion: 0.40
Nodes (7): TimetableController, Guid, HttpGet, HttpPost, IActionResult, Task, ValidateAntiForgeryToken

### Community 25 - "Premium ViewModels"
Cohesion: 0.20
Nodes (10): AttendanceMarkViewModel, CanMarkAbsent, CanMarkPresent, Date, EndTime, MarkedStatus, Room, StartTime (+2 more)

### Community 26 - "Admin ViewModels"
Cohesion: 0.20
Nodes (10): SubjectSummary, Absent, AttendancePercentage, Code, Color, Name, Present, Status (+2 more)

### Community 27 - "Attendance Mark ViewModels"
Cohesion: 0.20
Nodes (10): WeekendOpportunity, EndDate, HolidayName, IsSafe, LecturesMissed, ProjectedAttendance, StartDate, SubjectsAffected (+2 more)

### Community 28 - "Messages ViewModels"
Cohesion: 0.22
Nodes (9): ConversationViewModel, ConversationId, IsOnline, LastMessage, LastMessageAt, OtherUserId, OtherUserImage, OtherUserName (+1 more)

### Community 29 - "Planner Sim ViewModels"
Cohesion: 0.22
Nodes (9): ExtendedWeekendViewModel, HolidayDate, HolidayName, IsSafe, LecturesMissed, ProjectedAttendance, SubjectsAffected, WeekendEnd (+1 more)

### Community 30 - "Connect Directory ViewModels"
Cohesion: 0.22
Nodes (9): SubjectAnalytics, Absent, AttendancePercentage, Color, Present, SubjectId, SubjectName, TargetPercentage (+1 more)

### Community 31 - "Subjects ViewModels"
Cohesion: 0.22
Nodes (9): TodaysSchedule, AttendanceStatus, EndTime, HasAttendance, Room, StartTime, SubjectId, SubjectName (+1 more)

### Community 32 - "Calendar ViewModels"
Cohesion: 0.29
Nodes (7): SkipSimulatorViewModel, CurrentAttendance, CurrentPresentLectures, CurrentTotalLectures, Simulations, TargetPercentage, List

### Community 33 - "Attendance VMs + DateTime"
Cohesion: 0.33
Nodes (6): UpcomingHoliday, Date, DaysUntil, Name, Type, DateTime

### Community 34 - "Project & Dependencies"
Cohesion: 0.40
Nodes (4): net10.0, BCrypt.Net (0.1.0), Supabase (1.0.1), Microsoft.NET.Sdk.Web

### Community 35 - "ViewImports & Namespaces"
Cohesion: 0.50
Nodes (3): Attendly.Models, Attendly.ViewModels, Attendly.Configuration

## Knowledge Gaps
- **399 isolated node(s):** `net10.0`, `BCrypt.Net (0.1.0)`, `Supabase (1.0.1)`, `Microsoft.NET.Sdk.Web`, `Url` (+394 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **25 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `StudentProfile` connect `Student Profile Model` to `Dashboard Controller`, `Supabase Data Service`, `Analytics Controller`, `Supabase Interface`, `Premium & Razorpay`, `Admin Controller`, `Account Controller`, `Connect Controller`, `Profile Controller`, `Calendar Controller`, `Subject Model`, `Conversation & BaseModel`, `Message Model`?**
  _High betweenness centrality (0.146) - this node is a cross-community bridge._
- **Why does `ISupabaseService` connect `Dashboard Controller` to `Supabase Data Service`, `Analytics Controller`, `Supabase Interface`, `Premium & Razorpay`, `Admin Controller`, `Attendance Controller`, `Subjects Controller`, `Cross-Controller Refs`, `Account Controller`, `Connect Controller`, `Profile Controller`, `Calendar Controller`, `Timetable Controller`?**
  _High betweenness centrality (0.129) - this node is a cross-community bridge._
- **Why does `TimetableEntry` connect `Supabase Interface` to `Dashboard Controller`, `Supabase Data Service`, `Analytics Controller`, `Student Profile Model`, `Subject Model`, `Conversation & BaseModel`, `Timetable Controller`?**
  _High betweenness centrality (0.068) - this node is a cross-community bridge._
- **What connects `net10.0`, `BCrypt.Net (0.1.0)`, `Supabase (1.0.1)` to the rest of the system?**
  _399 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Dashboard Controller` be split into smaller, more focused modules?**
  _Cohesion score 0.07219548315438726 - nodes in this community are weakly interconnected._
- **Should `Supabase Data Service` be split into smaller, more focused modules?**
  _Cohesion score 0.06716417910447761 - nodes in this community are weakly interconnected._
- **Should `Analytics Controller` be split into smaller, more focused modules?**
  _Cohesion score 0.05565638233514821 - nodes in this community are weakly interconnected._