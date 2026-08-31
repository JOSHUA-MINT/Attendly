using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("calendar_events")]
public class CalendarEvent : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid UserId { get; set; }
    public StudentProfile? User { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = "personal";

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_all_day")]
    public bool IsAllDay { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
