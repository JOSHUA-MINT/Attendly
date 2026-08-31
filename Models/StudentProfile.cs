using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("student_profiles")]
public class StudentProfile : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("college")]
    public string? College { get; set; }

    [Column("course")]
    public string? Course { get; set; }

    [Column("year")]
    public string? Year { get; set; }

    [Column("division")]
    public string? Division { get; set; }

    [Column("railway_line")]
    public string? RailwayLine { get; set; }

    [Column("station")]
    public string? Station { get; set; }

    [Column("bio")]
    public string? Bio { get; set; }

    [Column("profile_image_url")]
    public string? ProfileImageUrl { get; set; }

    [Column("role")]
    public string? Role { get; set; } = "student";

    [Column("is_premium")]
    public bool IsPremium { get; set; } = false;

    [Column("premium_expires_at")]
    public DateTime? PremiumExpiresAt { get; set; }

    [Column("railway_line_group")]
    public string? RailwayLineGroup { get; set; }

    [Column("is_online")]
    public bool IsOnline { get; set; } = false;

    [Column("last_seen")]
    public DateTime? LastSeen { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
