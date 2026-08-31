using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace Attendly.Middleware;

public class SessionMiddleware
{
 private readonly RequestDelegate _next;

 public SessionMiddleware(RequestDelegate next)
 {
 _next = next;
 }

 public async Task InvokeAsync(HttpContext context)
 {
 if (context.Session.IsAvailable && context.User.Identity?.IsAuthenticated == true)
 {
 var userId = context.Session.GetString("UserId");
 if (string.IsNullOrEmpty(userId))
 {
 await context.SignOutAsync("Cookies");
 }
 }
 await _next(context);
 }
}
