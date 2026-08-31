using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("conversations")]
public class Conversation : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user1_id")]
    public Guid User1Id { get; set; }
    public StudentProfile? User1 { get; set; }

    [Column("user2_id")]
    public Guid User2Id { get; set; }
    public StudentProfile? User2 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_message_at")]
    public DateTime? LastMessageAt { get; set; }

    public List<Message>? Messages { get; set; }
}
