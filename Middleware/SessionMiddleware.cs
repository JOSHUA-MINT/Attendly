using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Attendly.Middleware;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionMiddleware> _logger;

    public SessionMiddleware(RequestDelegate next, ILogger<SessionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Session.IsAvailable && context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                var claimUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(claimUserId))
                {
                    context.Session.SetString("UserId", claimUserId);

                    var name = context.User.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(name))
                    {
                        context.Session.SetString("UserName", name);
                    }

                    var email = context.User.FindFirst("UserEmail")?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        context.Session.SetString("UserEmail", email);
                    }

                    var role = context.User.FindFirst("UserRole")?.Value;
                    if (!string.IsNullOrEmpty(role))
                    {
                        context.Session.SetString("UserRole", role);
                    }

                    var isPrem = context.User.FindFirst("IsPremium")?.Value;
                    if (!string.IsNullOrEmpty(isPrem))
                    {
                        context.Session.SetString("IsPremium", isPrem);
                    }

                    _logger.LogInformation("SessionMiddleware: synchronized session from claims for UserId={UserId}", claimUserId);
                }
            }
        }
        await _next(context);
    }
}
