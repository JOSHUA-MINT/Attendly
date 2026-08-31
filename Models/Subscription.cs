using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Attendly.Models;

[Table("subscriptions")]
public class Subscription : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid UserId { get; set; }
    public StudentProfile? User { get; set; }

    [Column("razorpay_order_id")]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [Column("razorpay_payment_id")]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    [Column("plan")]
    public string Plan { get; set; } = "monthly";

    [Column("discount_applied")]
    public int DiscountApplied { get; set; } = 0;

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("amount_paid")]
    public decimal AmountPaid { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
