using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("subjects")]
public class Subject : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public StudentProfile? User { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("code")]
    public string? Code { get; set; }

    [Column("professor")]
    public string? Professor { get; set; }

    [Column("room")]
    public string? Room { get; set; }

    [Column("lectures_per_week")]
    public int LecturesPerWeek { get; set; }

    [Column("target_percentage")]
    public int TargetPercentage { get; set; } = 75;

    [Column("color")]
    public string? Color { get; set; } = "#3B82F6";

    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public List<TimetableEntry>? TimetableEntries { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public List<AttendanceRecord>? AttendanceRecords { get; set; }
}
