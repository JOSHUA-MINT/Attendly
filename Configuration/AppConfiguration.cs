using System;
using System.Collections.Generic;
using System.Linq;

namespace Attendly.Configuration;

public static class SupabaseConfig
{
 public static string Url { get; set; } = string.Empty;
 public static string AnonKey { get; set; } = string.Empty;
 public static string ServiceRoleKey { get; set; } = string.Empty;

 public static bool IsConfigured =>
 !string.IsNullOrEmpty(Url) &&
 !string.IsNullOrEmpty(AnonKey) &&
 !string.IsNullOrEmpty(ServiceRoleKey);
}

public static class RazorpayConfig
{
 public static string KeyId { get; set; } = string.Empty;
 public static string KeySecret { get; set; } = string.Empty;

 public static bool IsConfigured =>
 !string.IsNullOrEmpty(KeyId) &&
 !string.IsNullOrEmpty(KeySecret);
}

public static class AppConfig
{
 public static string SiteName { get; set; } = "Attendly";
 public static string SupportEmail { get; set; } = "support@attendly.app";
}
