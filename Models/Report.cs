using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("reports")]
public class Report : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("reporter_id")]
    public Guid ReporterId { get; set; }
    public StudentProfile? Reporter { get; set; }

    [Column("reported_user_id")]
    public Guid ReportedUserId { get; set; }
    public StudentProfile? ReportedUser { get; set; }

    [Column("message_id")]
    public Guid? MessageId { get; set; }
    public Message? Message { get; set; }

    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("reviewed_by_admin_id")]
    public Guid? ReviewedByAdminId { get; set; }

    [Column("admin_notes")]
    public string? AdminNotes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("reviewed_at")]
    public DateTime? ReviewedAt { get; set; }
}
