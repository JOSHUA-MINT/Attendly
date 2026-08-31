using Attendly.Configuration;
using Attendly.Models;
using Supabase;
using Supabase.Postgrest;
using Supabase.Realtime;

namespace Attendly.Data;

public interface ISupabaseService
{
 Task InitializeAsync();
 Task<StudentProfile?> GetProfileByEmailAsync(string email);
 Task<StudentProfile?> GetProfileByIdAsync(Guid id);
 Task<StudentProfile> CreateProfileAsync(StudentProfile profile);
 Task UpdateProfileAsync(StudentProfile profile);
 Task<List<StudentProfile>> GetStudentsByRailwayLineAsync(string line);
 Task<List<StudentProfile>> SearchStudentsAsync(string query, string? line = null);
 Task<bool> DeleteProfileAsync(Guid id);

 Task<Subject> CreateSubjectAsync(Subject subject);
 Task<List<Subject>> GetUserSubjectsAsync(Guid userId, bool archived = false);
 Task<Subject?> GetSubjectAsync(Guid id, Guid userId);
 Task UpdateSubjectAsync(Subject subject);
 Task DeleteSubjectAsync(Guid id, Guid userId);

 Task<AttendanceRecord> CreateAttendanceAsync(AttendanceRecord record);
 Task<List<AttendanceRecord>> GetAttendanceRecordsAsync(Guid userId, Guid subjectId);
 Task<List<AttendanceRecord>> GetAttendanceForDateAsync(Guid userId, DateTime date);
 Task UpdateAttendanceAsync(AttendanceRecord record);
 Task DeleteAttendanceAsync(Guid id, Guid userId);
 Task<AttendanceRecord?> GetAttendanceForSlotAsync(Guid userId, Guid subjectId, DateTime date, int lectureNumber);

 Task<TimetableEntry> CreateTimetableEntryAsync(TimetableEntry entry);
 Task<List<TimetableEntry>> GetUserTimetableAsync(Guid userId);
 Task UpdateTimetableEntryAsync(TimetableEntry entry);
 Task DeleteTimetableEntryAsync(Guid id, Guid userId);

 Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent);
 Task<List<CalendarEvent>> GetUserEventsAsync(Guid userId, DateTime month);
 Task UpdateEventAsync(CalendarEvent calendarEvent);
 Task DeleteEventAsync(Guid id, Guid userId);

 Task<List<Holiday>> GetHolidaysAsync(DateTime? fromDate = null, DateTime? toDate = null);
 Task<Holiday> CreateHolidayAsync(Holiday holiday);
 Task UpdateHolidayAsync(Holiday holiday);
 Task DeleteHolidayAsync(Guid id);

 Task<Connection> CreateConnectionRequestAsync(Guid requesterId, Guid receiverId);
 Task<List<Connection>> GetUserConnectionsAsync(Guid userId);
 Task<Connection?> GetConnectionAsync(Guid requesterId, Guid receiverId);
 Task UpdateConnectionAsync(Connection connection);

 Task<Conversation> CreateConversationAsync(Guid user1Id, Guid user2Id);
 Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id);
 Task<List<Conversation>> GetUserConversationsAsync(Guid userId);

 Task<Message> CreateMessageAsync(Message message);
 Task<List<Message>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50);
 Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId);

 Task<Report> CreateReportAsync(Report report);
 Task<List<Report>> GetReportsAsync(string? status = null);
 Task UpdateReportAsync(Report report);

 Task<Subscription> CreateSubscriptionAsync(Subscription subscription);
 Task<Subscription?> GetUserSubscriptionAsync(Guid userId);
 Task UpdateSubscriptionAsync(Subscription subscription);

 Task<List<StudentProfile>> GetAllStudentsAsync(int page = 1, int pageSize = 50);
}
