using Attendly.Configuration;
using System.Text.Json;

namespace Attendly.Services;

public interface IRazorpayService
{
 Task<string> CreateOrderAsync(string userId, int amountInPaise, string receiptId, string plan);
 Task<bool> VerifySignatureAsync(string orderId, string paymentId, string signature);
}

public class RazorpayService : IRazorpayService
{
 private readonly HttpClient _httpClient;
 private readonly string _keyId;
 private readonly string _keySecret;

 public RazorpayService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
 {
 _keyId = configuration["Razorpay:KeyId"] ?? RazorpayConfig.KeyId;
 _keySecret = configuration["Razorpay:KeySecret"] ?? RazorpayConfig.KeySecret;
 _httpClient = httpClientFactory.CreateClient("Razorpay");
 }

 public async Task<string> CreateOrderAsync(string userId, int amountInPaise, string receiptId, string plan)
 {
 var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));

 var orderData = new
 {
 amount = amountInPaise,
 currency = "INR",
 receipt = receiptId,
 payment_capture = 1,
 notes = new { user_id = userId, plan = plan }
 };

 var request = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/orders")
 {
 Content = new StringContent(JsonSerializer.Serialize(orderData), System.Text.Encoding.UTF8, "application/json")
 };

 request.Headers.Add("Authorization", $"Basic {auth}");

 var response = await _httpClient.SendAsync(request);
 response.EnsureSuccessStatusCode();

 var responseBody = await response.Content.ReadAsStringAsync();
 return responseBody;
 }

 public async Task<bool> VerifySignatureAsync(string orderId, string paymentId, string signature)
 {
 using var hmac = new System.Security.Cryptography.HMACSHA256(
 System.Text.Encoding.UTF8.GetBytes(_keySecret));

 var payload = $"{orderId}|{paymentId}";
 var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
 var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

 return computedSignature == signature.ToLowerInvariant();
 }
}
