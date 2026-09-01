namespace Attendly.Configuration;

public static class RazorpayConfig
{
 public static string KeyId { get; set; } = "";
 public static string KeySecret { get; set; } = "";
 public static bool IsConfigured => !string.IsNullOrEmpty(KeyId) && !string.IsNullOrEmpty(KeySecret);
}
