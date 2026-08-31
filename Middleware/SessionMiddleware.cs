using Attendly.Data;
using Attendly.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Attendly.Middleware;

public class SessionMiddleware
{
 private readonly RequestDelegate _next;

 public SessionMiddleware(RequestDelegate next)
 {
 _next = next;
 }

 public async Task InvokeAsync(HttpContext context, ISupabaseService supabaseService)
 {
 var userId = context.Session.GetString("UserId");
 var userName = context.Session.GetString("UserName");

 if (!string.IsNullOrEmpty(userId))
 {
 context.Items["CurrentUserId"] = Guid.Parse(userId);
 context.Items["CurrentUserName"] = userName;
 }

 await _next(context);
 }
}
