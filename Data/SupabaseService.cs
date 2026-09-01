using Attendly.Configuration;
using Attendly.Models;
using Supabase;
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants;
using Supabase.Realtime;
using System.Text.Json;

namespace Attendly.Data;

public class SupabaseService : ISupabaseService, IDisposable, IAsyncDisposable
{
    private readonly Supabase.Client _client;

    public SupabaseService()
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        _client = new Supabase.Client(SupabaseConfig.Url, SupabaseConfig.ServiceRoleKey, options);
    }

    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
    }

    // ── Profiles ──
    public async Task<StudentProfile?> GetProfileByEmailAsync(string email)
    {
        try
        {
            var result = await _client.From<StudentProfile>()
                .Filter("email", Operator.Equals, email)
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch { return null; }
    }

    public async Task<StudentProfile?> GetProfileByIdAsync(Guid id)
    {
        try
        {
            var result = await _client.From<StudentProfile>()
                .Filter("id", Operator.Equals, id.ToString())
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetProfileByIdAsync ERROR] ID={id}: {ex.Message}");
            return null;
        }
    }

    public async Task<StudentProfile> CreateProfileAsync(StudentProfile profile)
    {
        var result = await _client.From<StudentProfile>()
            .Insert(profile);
        return result.Models.First();
    }

    public async Task UpdateProfileAsync(StudentProfile profile)
    {
        await _client.From<StudentProfile>()
            .Update(profile);
    }

    public async Task<List<StudentProfile>> GetStudentsByRailwayLineAsync(string line)
    {
        var result = await _client.From<StudentProfile>()
            .Filter("railway_line", Operator.Equals, line)
            .Filter("is_active", Operator.Equals, "true")
            .Get();
        return result.Models.ToList();
    }

    public async Task<List<StudentProfile>> SearchStudentsAsync(string query, string? line = null)
    {
        var queryBuilder = _client.From<StudentProfile>()
            .Filter("is_active", Operator.Equals, "true")
            .Filter("role", Operator.Equals, "student");

        if (!string.IsNullOrEmpty(line))
        {
            queryBuilder = queryBuilder.Filter("railway_line", Operator.Equals, line);
        }

        if (!string.IsNullOrEmpty(query))
        {
            queryBuilder = queryBuilder.Filter("full_name", Operator.ILike, $"%{query}%");
        }

        var result = await queryBuilder.Get();
        return result.Models.ToList();
    }

    public async Task<bool> DeleteProfileAsync(Guid id)
    {
        try
        {
            await _client.From<StudentProfile>()
                .Delete(new StudentProfile { Id = id });
            return true;
        }
        catch { return false; }
    }

    // ── Subjects ──
    public async Task<Subject> CreateSubjectAsync(Subject subject)
    {
        var result = await _client.From<Subject>()
            .Insert(subject);
        return result.Models.First();
    }

    public async Task<List<Subject>> GetUserSubjectsAsync(Guid userId, bool archived = false)
    {
        var result = await _client.From<Subject>()
            .Filter("user_id", Operator.Equals, userId.ToString())
            .Filter("is_archived", Operator.Equals, archived ? "true" : "false")
            .Order("created_at", Ordering.Descending)
            .Get();
        return result.Models.ToList();
    }

    public async Task<Subject?> GetSubjectAsync(Guid id, Guid userId)
    {
        try
        {
            var result = await _client.From<Subject>()
                .Filter("id", Operator.Equals, id.ToString())
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch { return null; }
    }

    public async Task UpdateSubjectAsync(Subject subject)
    {
        await _client.From<Subject>()
            .Update(subject);
    }

    public async Task DeleteSubjectAsync(Guid id, Guid userId)
    {
        await _client.From<Subject>()
            .Delete(new Subject { Id = id, UserId = userId });
    }

    // ── Attendance ──
    public async Task<AttendanceRecord> CreateAttendanceAsync(AttendanceRecord record)
    {
        var result = await _client.From<AttendanceRecord>()
            .Insert(record);
        return result.Models.First();
    }

    public async Task<List<AttendanceRecord>> GetAttendanceRecordsAsync(Guid userId, Guid subjectId)
    {
        var result = await _client.From<AttendanceRecord>()
            .Filter("user_id", Operator.Equals, userId.ToString())
            .Filter("subject_id", Operator.Equals, subjectId.ToString())
            .Order("date", Ordering.Descending)
            .Get();
        return result.Models.ToList();
    }

    public async Task<List<AttendanceRecord>> GetAttendanceForDateAsync(Guid userId, DateTime date)
    {
        var start = date.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var end = date.Date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ");
        var result = await _client.From<AttendanceRecord>()
            .Filter("user_id", Operator.Equals, userId.ToString())
            .Filter("date", Operator.GreaterThanOrEqual, start)
            .Filter("date", Operator.LessThan, end)
            .Get();
        return result.Models.ToList();
    }

    public async Task<AttendanceRecord?> GetAttendanceByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var result = await _client.From<AttendanceRecord>()
                .Filter("id", Operator.Equals, id.ToString())
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch { return null; }
    }

    public async Task UpdateAttendanceAsync(AttendanceRecord record)
    {
        await _client.From<AttendanceRecord>()
            .Update(record);
    }

    public async Task DeleteAttendanceAsync(Guid id, Guid userId)
    {
        await _client.From<AttendanceRecord>()
            .Filter("id", Operator.Equals, id.ToString())
            .Filter("user_id", Operator.Equals, userId.ToString())
            .Delete();
    }

    public async Task<AttendanceRecord?> GetAttendanceForSlotAsync(Guid userId, Guid subjectId, DateTime date, int lectureNumber)
    {
        try
        {
            var start = date.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var end = date.Date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var result = await _client.From<AttendanceRecord>()
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Filter("subject_id", Operator.Equals, subjectId.ToString())
                .Filter("date", Operator.GreaterThanOrEqual, start)
                .Filter("date", Operator.LessThan, end)
                .Filter("lecture_number", Operator.Equals, lectureNumber)
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch { return null; }
    }

    // ── Timetable ──
    public async Task<TimetableEntry> CreateTimetableEntryAsync(TimetableEntry entry)
    {
        var result = await _client.From<TimetableEntry>()
            .Insert(entry);
        return result.Models.First();
    }

    public async Task<List<TimetableEntry>> GetUserTimetableAsync(Guid userId)
    {
        var result = await _client.From<TimetableEntry>()
            .Filter("user_id", Operator.Equals, userId.ToString())
            .Get();
        return result.Models.ToList();
    }

    public async Task UpdateTimetableEntryAsync(TimetableEntry entry)
    {
        await _client.From<TimetableEntry>()
            .Update(entry);
    }

    public async Task DeleteTimetableEntryAsync(Guid id, Guid userId)
    {
        await _client.From<TimetableEntry>()
            .Delete(new TimetableEntry { Id = id, UserId = userId });
    }

    // ── Calendar Events ──
    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
    {
        var result = await _client.From<CalendarEvent>()
            .Insert(calendarEvent);
        return result.Models.First();
    }

    public async Task<List<CalendarEvent>> GetUserEventsAsync(Guid userId, DateTime month)
    {
        var start = new DateTime(month.Year, month.Month, 1).ToString("yyyy-MM-ddTHH:mm:ssZ");
        var end = new DateTime(month.Year, month.Month, 1).AddMonths(1).ToString("yyyy-MM-ddTHH:mm:ssZ");
        var result = await _client.From<CalendarEvent>()
            .Filter("user_id", Operator.Equals, userId.ToString())
            .Filter("date", Operator.GreaterThanOrEqual, start)
            .Filter("date", Operator.LessThan, end)
            .Get();
        return result.Models.ToList();
    }

    public async Task UpdateEventAsync(CalendarEvent calendarEvent)
    {
        await _client.From<CalendarEvent>()
            .Update(calendarEvent);
    }

    public async Task DeleteEventAsync(Guid id, Guid userId)
    {
        await _client.From<CalendarEvent>()
            .Delete(new CalendarEvent { Id = id, UserId = userId });
    }

    // ── Holidays ──
    public async Task<List<Holiday>> GetHolidaysAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _client.From<Holiday>();
        query.Filter("is_active", Operator.Equals, "true");

        if (fromDate.HasValue)
        {
            query.Filter("date", Operator.GreaterThanOrEqual, fromDate.Value.ToString("yyyy-MM-dd"));
        }
        if (toDate.HasValue)
        {
            query.Filter("date", Operator.LessThanOrEqual, toDate.Value.ToString("yyyy-MM-dd"));
        }

        var result = await query.Order("date", Ordering.Ascending).Get();
        return result.Models.ToList();
    }

    public async Task<Holiday> CreateHolidayAsync(Holiday holiday)
    {
        var result = await _client.From<Holiday>()
            .Insert(holiday);
        return result.Models.First();
    }

    public async Task UpdateHolidayAsync(Holiday holiday)
    {
        await _client.From<Holiday>()
            .Update(holiday);
    }

    public async Task DeleteHolidayAsync(Guid id)
    {
        await _client.From<Holiday>()
            .Delete(new Holiday { Id = id });
    }

    // ── Connections ──
    public async Task<Connection> CreateConnectionRequestAsync(Guid requesterId, Guid receiverId)
    {
        var existing = await GetConnectionAsync(requesterId, receiverId);
        if (existing != null)
        {
            if (existing.Status == "rejected")
            {
                existing.Status = "pending";
                existing.UpdatedAt = DateTime.UtcNow;
                await UpdateConnectionAsync(existing);
                return existing;
            }
            return existing;
        }

        var connection = new Connection
        {
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = "pending"
        };

        var result = await _client.From<Connection>()
            .Insert(connection);
        return result.Models.First();
    }

    public async Task<List<Connection>> GetUserConnectionsAsync(Guid userId)
    {
        var result = await _client.From<Connection>()
            .Filter("requester_id", Operator.Equals, userId.ToString())
            .Get();
        var list = result.Models.ToList();

        var result2 = await _client.From<Connection>()
            .Filter("receiver_id", Operator.Equals, userId.ToString())
            .Get();
        list.AddRange(result2.Models);

        return list;
    }

    public async Task<Connection?> GetConnectionAsync(Guid requesterId, Guid receiverId)
    {
        try
        {
            var result = await _client.From<Connection>()
                .Filter("requester_id", Operator.Equals, requesterId.ToString())
                .Filter("receiver_id", Operator.Equals, receiverId.ToString())
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch { return null; }
    }

    public async Task UpdateConnectionAsync(Connection connection)
    {
        await _client.From<Connection>()
            .Update(connection);
    }

    // ── Conversations ──
    public async Task<Conversation> CreateConversationAsync(Guid user1Id, Guid user2Id)
    {
        var minId = user1Id < user2Id ? user1Id : user2Id;
        var maxId = user1Id < user2Id ? user2Id : user1Id;
        var conv = new Conversation
        {
            User1Id = minId,
            User2Id = maxId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _client.From<Conversation>()
            .Insert(conv);
        return result.Models.First();
    }

    public async Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id)
    {
        var minId = user1Id < user2Id ? user1Id : user2Id;
        var maxId = user1Id < user2Id ? user2Id : user1Id;
        try
        {
            var result = await _client.From<Conversation>()
                .Filter("user1_id", Operator.Equals, minId.ToString())
                .Filter("user2_id", Operator.Equals, maxId.ToString())
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch { return null; }
    }

    public async Task<List<Conversation>> GetUserConversationsAsync(Guid userId)
    {
        var result = await _client.From<Conversation>()
            .Filter("user1_id", Operator.Equals, userId.ToString())
            .Get();
        var list = result.Models.ToList();

        var result2 = await _client.From<Conversation>()
            .Filter("user2_id", Operator.Equals, userId.ToString())
            .Get();
        list.AddRange(result2.Models);

        return list;
    }

    // ── Messages ──
    public async Task<Message> CreateMessageAsync(Message message)
    {
        var result = await _client.From<Message>()
            .Insert(message);
        return result.Models.First();
    }

    public async Task<List<Message>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50)
    {
        var offset = (page - 1) * pageSize;
        var result = await _client.From<Message>()
            .Filter("conversation_id", Operator.Equals, conversationId.ToString())
            .Filter("is_deleted", Operator.Equals, "false")
            .Order("created_at", Ordering.Descending)
            .Range(offset, offset + pageSize - 1)
            .Get();
        var messages = result.Models.OrderBy(m => m.CreatedAt).ToList();
        return messages;
    }

    public async Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId)
    {
        var messages = await _client.From<Message>()
            .Filter("conversation_id", Operator.Equals, conversationId.ToString())
            .Filter("sender_id", Operator.NotEqual, userId.ToString())
            .Get();

        foreach (var msg in messages.Models.Where(m => m.ReadAt == null))
        {
            msg.ReadAt = DateTime.UtcNow;
            await _client.From<Message>().Update(msg);
        }
    }

    // ── Reports ──
    public async Task<Report> CreateReportAsync(Report report)
    {
        var result = await _client.From<Report>()
            .Insert(report);
        return result.Models.First();
    }

    public async Task<List<Report>> GetReportsAsync(string? status = null)
    {
        var query = _client.From<Report>();
        if (!string.IsNullOrEmpty(status))
        {
            query.Filter("status", Operator.Equals, status);
        }
        var result = await query.Order("created_at", Ordering.Descending).Get();
        return result.Models.ToList();
    }

    public async Task UpdateReportAsync(Report report)
    {
        await _client.From<Report>()
            .Update(report);
    }

    // ── Subscriptions ──
    public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
    {
        var result = await _client.From<Subscription>()
            .Insert(subscription);
        return result.Models.First();
    }

    public async Task<Subscription?> GetUserSubscriptionAsync(Guid userId)
    {
        try
        {
            var result = await _client.From<Subscription>()
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Filter("status", Operator.Equals, "active")
                .Order("created_at", Ordering.Descending)
                .Range(0, 0)
                .Get();
            return result.Models.FirstOrDefault();
        }
        catch { return null; }
    }

    public async Task UpdateSubscriptionAsync(Subscription subscription)
    {
        await _client.From<Subscription>()
            .Update(subscription);
    }

    // ── Admin / All Students ──
    public async Task<List<StudentProfile>> GetAllStudentsAsync(int page = 1, int pageSize = 50)
    {
        var offset = (page - 1) * pageSize;
        var result = await _client.From<StudentProfile>()
            .Order("created_at", Ordering.Descending)
            .Range(offset, offset + pageSize - 1)
            .Get();
        return result.Models.ToList();
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
