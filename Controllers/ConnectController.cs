using Attendly.Data;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("connect")]
public class ConnectController : Controller
{
 private readonly ISupabaseService _supabase;

 public ConnectController(ISupabaseService supabase)
 {
 _supabase = supabase;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index(string? line = "western", string? search = null, string? college = null)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var students = await _supabase.SearchStudentsAsync(search ?? "", line);
 var connections = await _supabase.GetUserConnectionsAsync(userId.Value);

 var viewModels = students.Select(s =>
 {
 var myConnections = connections.FirstOrDefault(c =>
 (c.RequesterId == userId && c.ReceiverId == s.Id) ||
 (c.ReceiverId == userId && c.RequesterId == s.Id));

 return new StudentDirectoryViewModel
 {
 Id = s.Id,
 FullName = s.FullName ?? "Student",
 ProfileImageUrl = s.ProfileImageUrl,
 College = s.College,
 Course = s.Course,
 Year = s.Year,
 Station = s.Station,
 IsOnline = s.IsOnline,
 Bio = s.Bio,
 ConnectionStatus = myConnections?.Status ?? "none"
 };
 }).ToList();

 ViewBag.RailwayLine = line ?? "western";
 ViewBag.LineLabel = line == "central" ? "Central Line" : "Western Line";

 ViewBag.Colleges = viewModels.Select(v => v.College).Distinct().Where(c => !string.IsNullOrEmpty(c)).ToList();
 ViewBag.Courses = viewModels.Select(v => v.Course).Distinct().Where(c => !string.IsNullOrEmpty(c)).ToList();
 ViewBag.Years = viewModels.Select(v => v.Year).Distinct().Where(c => !string.IsNullOrEmpty(c)).ToList();
 ViewBag.Stations = viewModels.Select(v => v.Station).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList();

 return View(viewModels);
 }

 [HttpPost("connect/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> SendRequest(Guid id)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");
 if (id == userId) return RedirectToAction("Index");

 var existing = await _supabase.GetConnectionAsync(userId.Value, id);
 if (existing == null || existing.Status == "rejected")
 {
 existing = new Models.Connection
 {
 RequesterId = userId.Value,
 ReceiverId = id,
 Status = "pending"
 };
 await _supabase.CreateConnectionRequestAsync(userId.Value, id);
 TempData["Success"] = "Connection request sent!";
 }
 else if (existing.Status == "pending")
 {
 TempData["Info"] = "Request already sent.";
 }

 return RedirectToAction("Index");
 }

 [HttpPost("respond/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Respond(Guid id, string response)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var connection = await _supabase.GetConnectionAsync(id, userId.Value);
 if (connection == null)
 {
 connection = await _supabase.GetConnectionAsync(userId.Value, id);
 }

 if (connection == null) return NotFound();

 if (response == "accept")
 {
 connection.Status = "accepted";
 var conversation = await _supabase.GetConversationAsync(connection.RequesterId, connection.ReceiverId);
 if (conversation == null)
 {
 conversation = await _supabase.CreateConversationAsync(connection.RequesterId, connection.ReceiverId);
 connection.ConversationId = conversation.Id;
 }
 TempData["Success"] = "Connection accepted!";
 }
 else
 {
 connection.Status = "rejected";
 TempData["Info"] = "Request declined.";
 }

 connection.UpdatedAt = DateTime.UtcNow;
 connection.RespondedAt = DateTime.UtcNow;
 await _supabase.UpdateConnectionAsync(connection);

 return RedirectToAction("Connections");
 }

 [HttpGet("connections")]
 public async Task<IActionResult> Connections()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var connections = await _supabase.GetUserConnectionsAsync(userId.Value);
 var accepted = connections.Where(c => c.Status == "accepted").ToList();
 var pending = connections.Where(c => c.Status == "pending" && c.ReceiverId == userId).ToList();

 ViewBag.Pending = pending;
 return View(accepted);
 }

 [HttpPost("remove/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> RemoveConnection(Guid id)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var connection = await _supabase.GetConnectionAsync(userId.Value, id)
 ?? await _supabase.GetConnectionAsync(id, userId.Value);

 if (connection != null)
 {
 connection.Status = "rejected";
 await _supabase.UpdateConnectionAsync(connection);
 TempData["Success"] = "Connection removed.";
 }

 return RedirectToAction("Connections");
 }

    private Guid? GetCurrentUserId()
    {
        var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(claimId) && Guid.TryParse(claimId, out var claimGuid))
        {
            return claimGuid;
        }
        var id = HttpContext.Session.GetString("UserId");
        if (Guid.TryParse(id, out var guid)) return guid;
        return null;
    }
}
