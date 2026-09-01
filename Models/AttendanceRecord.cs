using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("attendance_records")]
public class AttendanceRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public StudentProfile? User { get; set; }

    [Column("subject_id")]
    public Guid SubjectId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public Subject? Subject { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("status")]
    public string Status { get; set; } = "present";

    [Column("lecture_number")]
    public int? LectureNumber { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
