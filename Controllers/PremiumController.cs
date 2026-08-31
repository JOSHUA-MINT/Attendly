using Attendly.Data;
using Attendly.Models;
using Attendly.Services;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("premium")]
public class PremiumController : Controller
{
 private readonly ISupabaseService _supabase;
 private readonly IRazorpayService _razorpay;

 public PremiumController(ISupabaseService supabase, IRazorpayService razorpay)
 {
 _supabase = supabase;
 _razorpay = razorpay;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile == null) return RedirectToAction("Login", "Account");

 var subscription = await _supabase.GetUserSubscriptionAsync(userId.Value);
 bool isPremium = false;
 DateTime? expiresAt = null;

 if (subscription != null && subscription.Status == "active" && subscription.ExpiresAt > DateTime.UtcNow)
 {
 isPremium = true;
 expiresAt = subscription.ExpiresAt;
 profile.IsPremium = true;
 profile.PremiumExpiresAt = expiresAt;
 await _supabase.UpdateProfileAsync(profile);
 }

 var vm = new PremiumViewModel
 {
 IsPremium = isPremium,
 PremiumExpiresAt = expiresAt,
 RazorpayKeyId = Configuration.RazorpayConfig.KeyId,
 MonthlyPrice = 49,
 YearlyPrice = 399
 };

 return View(vm);
 }

 [HttpPost("create-order")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> CreateOrder(string plan)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return Json(new { success = false, message = "Not authenticated" });

 if (!Configuration.RazorpayConfig.IsConfigured)
 {
 return Json(new { success = false, message = "Payment system not configured" });
 }

 int amount = plan == "yearly" ? 39900 : 4900; // in paise
 string receiptId = $"attendly_{userId.Value.ToString("N")}_{DateTime.UtcNow:yyyyMMddHHmmss}";

 try
 {
 var orderJson = await _razorpay.CreateOrderAsync(userId.Value.ToString(), amount, receiptId, plan);
 return Content(orderJson, "application/json");
 }
 catch (Exception ex)
 {
 return Json(new { success = false, message = ex.Message });
 }
 }

 [HttpPost("verify-payment")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> VerifyPayment(string razorpay_order_id, string razorpay_payment_id, string razorpay_signature, string plan)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Index", "Premium");

 var isValid = await _razorpay.VerifySignatureAsync(razorpay_order_id, razorpay_payment_id, razorpay_signature);
 if (!isValid)
 {
 TempData["Error"] = "Payment verification failed. Please contact support.";
 return RedirectToAction("Index");
 }

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile == null) return RedirectToAction("Login", "Account");

 DateTime now = DateTime.UtcNow;
 DateTime expiresAt = plan == "yearly" ? now.AddYears(1) : now.AddMonths(1);

 // Create subscription
 var subscription = new Subscription
 {
 UserId = userId.Value,
 RazorpayOrderId = razorpay_order_id,
 RazorpayPaymentId = razorpay_payment_id,
 Plan = plan,
 Status = "active",
 AmountPaid = plan == "yearly" ? 399 : 49,
 StartedAt = now,
 ExpiresAt = expiresAt
 };

 await _supabase.CreateSubscriptionAsync(subscription);

 profile.IsPremium = true;
 profile.PremiumExpiresAt = expiresAt;
 await _supabase.UpdateProfileAsync(profile);

 TempData["Success"] = plan == "yearly"
 ? "Welcome to Attendly Premium! Your yearly subscription is now active."
 : "Welcome to Attendly Premium! Your monthly subscription is now active.";

 return RedirectToAction("Index");
 }

 private Guid? GetCurrentUserId()
 {
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }
}
