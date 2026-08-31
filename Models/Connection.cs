using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("connections")]
public class Connection : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("requester_id")]
    public Guid RequesterId { get; set; }
    public StudentProfile? Requester { get; set; }

    [Column("receiver_id")]
    public Guid ReceiverId { get; set; }
    public StudentProfile? Receiver { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("conversation_id")]
    public Guid? ConversationId { get; set; }
    public Conversation? Conversation { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("responded_at")]
    public DateTime? RespondedAt { get; set; }
}
