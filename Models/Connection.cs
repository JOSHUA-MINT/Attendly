using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
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

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public StudentProfile? Requester { get; set; }

    [Column("receiver_id")]
    public Guid ReceiverId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public StudentProfile? Receiver { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("conversation_id")]
    public Guid? ConversationId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public Conversation? Conversation { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("responded_at")]
    public DateTime? RespondedAt { get; set; }
}
