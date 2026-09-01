using Attendly.Data;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("profile")]
public class ProfileController : Controller
{
 private readonly ISupabaseService _supabase;

 public ProfileController(ISupabaseService supabase)
 {
 _supabase = supabase;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value);
 var connections = (await _supabase.GetUserConnectionsAsync(userId.Value))
 .Count(c => c.Status == "accepted");

 var vm = new ProfileViewModel
 {
 Id = profile.Id,
 FullName = profile.FullName ?? "",
 Email = profile.Email,
 College = profile.College,
 Course = profile.Course,
 Year = profile.Year,
 Division = profile.Division,
 RailwayLine = profile.RailwayLine,
 Station = profile.Station,
 Bio = profile.Bio,
 ProfileImageUrl = profile.ProfileImageUrl,
 IsPremium = profile.IsPremium && profile.PremiumExpiresAt > DateTime.UtcNow,
 PremiumExpiresAt = profile.PremiumExpiresAt,
 TotalSubjects = subjects.Count,
 TotalConnections = connections
 };

 return View(vm);
 }

 [HttpGet("edit")]
 public async Task<IActionResult> Edit()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile == null) return RedirectToAction("Login", "Account");

 var vm = new ProfileViewModel
 {
 Id = profile.Id,
 FullName = profile.FullName ?? "",
 Email = profile.Email,
 College = profile.College,
 Course = profile.Course,
 Year = profile.Year,
 Division = profile.Division,
 RailwayLine = profile.RailwayLine,
 Station = profile.Station,
 Bio = profile.Bio,
 ProfileImageUrl = profile.ProfileImageUrl,
 IsPremium = profile.IsPremium && profile.PremiumExpiresAt > DateTime.UtcNow,
 PremiumExpiresAt = profile.PremiumExpiresAt
 };

 return View(vm);
 }

 [HttpPost("edit")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(ProfileViewModel model)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 if (!ModelState.IsValid)
 return View(model);

 var profile = await _supabase.GetProfileByIdAsync(userId.Value);
 if (profile == null) return RedirectToAction("Login", "Account");

 profile.FullName = model.FullName;
 profile.College = model.College;
 profile.Course = model.Course;
 profile.Year = model.Year;
 profile.Division = model.Division;
 profile.RailwayLine = model.RailwayLine;
 profile.Station = model.Station;
 profile.Bio = model.Bio;
 profile.UpdatedAt = DateTime.UtcNow;

 await _supabase.UpdateProfileAsync(profile);
 TempData["Success"] = "Profile updated successfully!";
 return RedirectToAction("Index");
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> ViewProfile(Guid id)
 {
 var profile = await _supabase.GetProfileByIdAsync(id);
 if (profile == null) return NotFound();


 var vm = new ProfileViewModel
 {
 Id = profile.Id,
 FullName = profile.FullName ?? "Student",
 Email = "", // Don't show email publicly
 College = profile.College,
 Course = profile.Course,
 Year = profile.Year,
 RailwayLine = profile.RailwayLine,
 Station = profile.Station,
 Bio = profile.Bio,
 ProfileImageUrl = profile.ProfileImageUrl,
 IsPremium = profile.IsPremium && profile.PremiumExpiresAt > DateTime.UtcNow
 };

 return View("Public", vm);
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
