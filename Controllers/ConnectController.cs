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
    public async Task<IActionResult> Index(string? line = "western", string? search = null, string? college = null, string? course = null, string? station = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var students = await _supabase.SearchStudentsAsync(search ?? "", line);
        var connections = await _supabase.GetUserConnectionsAsync(userId.Value);

        // Exclude current user from student directory
        var otherStudents = students.Where(s => s.Id != userId.Value).ToList();

        // Apply filters if provided
        if (!string.IsNullOrEmpty(college))
        {
            otherStudents = otherStudents.Where(s => string.Equals(s.College, college, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrEmpty(course))
        {
            otherStudents = otherStudents.Where(s => string.Equals(s.Course, course, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrEmpty(station))
        {
            otherStudents = otherStudents.Where(s => string.Equals(s.Station, station, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var viewModels = otherStudents.Select(s =>
        {
            var myConnection = connections.FirstOrDefault(c =>
                (c.RequesterId == userId && c.ReceiverId == s.Id) ||
                (c.ReceiverId == userId && c.RequesterId == s.Id));

            string status = "none";
            if (myConnection != null)
            {
                if (myConnection.Status == "accepted")
                {
                    status = "accepted";
                }
                else if (myConnection.Status == "pending")
                {
                    status = myConnection.RequesterId == userId ? "pending_outgoing" : "pending_incoming";
                }
            }

            return new StudentDirectoryViewModel
            {
                Id = s.Id,
                FullName = s.FullName ?? "Student",
                ProfileImageUrl = s.ProfileImageUrl,
                College = s.College,
                Course = s.Course,
                Year = s.Year,
                Station = s.Station,
                RailwayLine = s.RailwayLine,
                IsOnline = s.IsOnline,
                Bio = s.Bio,
                ConnectionStatus = status
            };
        }).ToList();

        ViewBag.RailwayLine = line ?? "western";
        ViewBag.LineLabel = string.Equals(line, "central", StringComparison.OrdinalIgnoreCase) ? "Central Line" : "Western Line";
        ViewBag.SearchQuery = search;
        ViewBag.FilterCollege = college;
        ViewBag.FilterCourse = course;
        ViewBag.FilterStation = station;

        ViewBag.Colleges = students.Select(v => v.College).Distinct().Where(c => !string.IsNullOrEmpty(c)).ToList();
        ViewBag.Courses = students.Select(v => v.Course).Distinct().Where(c => !string.IsNullOrEmpty(c)).ToList();
        ViewBag.Stations = students.Select(v => v.Station).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList();

        return View(viewModels);
    }

    [HttpPost("connect/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");
        if (id == userId) return RedirectToAction("Index");

        var existing = await _supabase.GetConnectionAsync(userId.Value, id) 
            ?? await _supabase.GetConnectionAsync(id, userId.Value);

        if (existing == null || existing.Status == "rejected")
        {
            await _supabase.CreateConnectionRequestAsync(userId.Value, id);
            TempData["Success"] = "Connection request sent!";
        }
        else if (existing.Status == "pending")
        {
            TempData["Info"] = "Connection request already pending.";
        }
        else if (existing.Status == "accepted")
        {
            TempData["Info"] = "You are already connected!";
        }

        return RedirectToAction("Index");
    }

    [HttpPost("respond/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(Guid id, string response)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var connection = await _supabase.GetConnectionAsync(id, userId.Value)
            ?? await _supabase.GetConnectionAsync(userId.Value, id);

        if (connection == null) return NotFound();

        if (string.Equals(response, "accept", StringComparison.OrdinalIgnoreCase))
        {
            connection.Status = "accepted";
            var conversation = await _supabase.GetConversationAsync(connection.RequesterId, connection.ReceiverId);
            if (conversation == null)
            {
                conversation = await _supabase.CreateConversationAsync(connection.RequesterId, connection.ReceiverId);
            }
            connection.ConversationId = conversation.Id;
            TempData["Success"] = "Connection accepted! You can now message each other.";
        }
        else
        {
            connection.Status = "rejected";
            TempData["Info"] = "Connection request declined.";
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
        var acceptedList = connections.Where(c => c.Status == "accepted").ToList();
        var pendingIncomingList = connections.Where(c => c.Status == "pending" && c.ReceiverId == userId).ToList();
        var pendingOutgoingList = connections.Where(c => c.Status == "pending" && c.RequesterId == userId).ToList();

        var model = new ConnectionsPageViewModel();

        foreach (var c in acceptedList)
        {
            var otherId = c.RequesterId == userId.Value ? c.ReceiverId : c.RequesterId;
            var otherProfile = await _supabase.GetProfileByIdAsync(otherId);
            model.Accepted.Add(new ConnectionItemViewModel
            {
                ConnectionId = c.Id,
                OtherUserId = otherId,
                OtherUserName = otherProfile?.FullName ?? "Student",
                OtherUserImage = otherProfile?.ProfileImageUrl,
                College = otherProfile?.College,
                Course = otherProfile?.Course,
                Year = otherProfile?.Year,
                Station = otherProfile?.Station,
                RailwayLine = otherProfile?.RailwayLine,
                Bio = otherProfile?.Bio,
                IsOnline = otherProfile?.IsOnline ?? false,
                Status = "accepted",
                CreatedAt = c.CreatedAt
            });
        }

        foreach (var c in pendingIncomingList)
        {
            var otherProfile = await _supabase.GetProfileByIdAsync(c.RequesterId);
            model.PendingIncoming.Add(new ConnectionItemViewModel
            {
                ConnectionId = c.Id,
                OtherUserId = c.RequesterId,
                OtherUserName = otherProfile?.FullName ?? "Student",
                OtherUserImage = otherProfile?.ProfileImageUrl,
                College = otherProfile?.College,
                Course = otherProfile?.Course,
                Year = otherProfile?.Year,
                Station = otherProfile?.Station,
                RailwayLine = otherProfile?.RailwayLine,
                Bio = otherProfile?.Bio,
                IsOnline = otherProfile?.IsOnline ?? false,
                Status = "pending",
                IsIncoming = true,
                CreatedAt = c.CreatedAt
            });
        }

        foreach (var c in pendingOutgoingList)
        {
            var otherProfile = await _supabase.GetProfileByIdAsync(c.ReceiverId);
            model.PendingOutgoing.Add(new ConnectionItemViewModel
            {
                ConnectionId = c.Id,
                OtherUserId = c.ReceiverId,
                OtherUserName = otherProfile?.FullName ?? "Student",
                OtherUserImage = otherProfile?.ProfileImageUrl,
                College = otherProfile?.College,
                Course = otherProfile?.Course,
                Year = otherProfile?.Year,
                Station = otherProfile?.Station,
                RailwayLine = otherProfile?.RailwayLine,
                Bio = otherProfile?.Bio,
                Status = "pending",
                IsIncoming = false,
                CreatedAt = c.CreatedAt
            });
        }

        return View(model);
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
            connection.UpdatedAt = DateTime.UtcNow;
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
