using Attendly.Data;
using Attendly.Services;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Attendly.Controllers;

[Route("subjects")]
public class SubjectsController : Controller
{
 private readonly ISupabaseService _supabase;

 public SubjectsController(ISupabaseService supabase)
 {
 _supabase = supabase;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subjects = await _supabase.GetUserSubjectsAsync(userId.Value, archived: false);
 var viewModels = subjects.Select(s => new SubjectViewModel
 {
 Id = s.Id,
 Name = s.Name,
 Code = s.Code,
 Professor = s.Professor,
 Room = s.Room,
 LecturesPerWeek = s.LecturesPerWeek,
 TargetPercentage = s.TargetPercentage,
 Color = s.Color,
 IsArchived = s.IsArchived,
 TimetableEntriesCount = s.TimetableEntries?.Count ?? 0
 }).ToList();

 return View(viewModels);
 }

 [HttpGet("create")]
 public IActionResult Create()
 {
 return View(new SubjectCreateViewModel());
 }

 [HttpPost("create")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Create(SubjectCreateViewModel model)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 if (!ModelState.IsValid)
 return View(model);

 var subject = new Models.Subject
 {
 UserId = userId.Value,
 Name = model.Name,
 Code = model.Code,
 Professor = model.Professor,
 Room = model.Room,
 LecturesPerWeek = model.LecturesPerWeek,
 TargetPercentage = model.TargetPercentage,
 Color = model.Color
 };

 await _supabase.CreateSubjectAsync(subject);
 TempData["Success"] = "Subject added successfully!";
 return RedirectToAction("Index");
 }

 [HttpGet("{id}/edit")]
 public async Task<IActionResult> Edit(Guid id)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subject = await _supabase.GetSubjectAsync(id, userId.Value);
 if (subject == null) return NotFound();

 var model = new SubjectCreateViewModel
 {
 Name = subject.Name,
 Code = subject.Code,
 Professor = subject.Professor,
 Room = subject.Room,
 LecturesPerWeek = subject.LecturesPerWeek,
 TargetPercentage = subject.TargetPercentage,
 Color = subject.Color
 };

 ViewBag.SubjectId = id;
 ViewBag.SubjectName = subject.Name;
 return View(model);
 }

 [HttpPost("{id}/edit")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(Guid id, SubjectCreateViewModel model)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 if (!ModelState.IsValid)
 {
 ViewBag.SubjectId = id;
 return View(model);
 }

 var subject = await _supabase.GetSubjectAsync(id, userId.Value);
 if (subject == null) return NotFound();

 subject.Name = model.Name;
 subject.Code = model.Code;
 subject.Professor = model.Professor;
 subject.Room = model.Room;
 subject.LecturesPerWeek = model.LecturesPerWeek;
 subject.TargetPercentage = model.TargetPercentage;
 subject.Color = model.Color;
 subject.UpdatedAt = DateTime.UtcNow;

 await _supabase.UpdateSubjectAsync(subject);
 TempData["Success"] = "Subject updated successfully!";
 return RedirectToAction("Index");
 }

 [HttpPost("{id}/archive")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Archive(Guid id)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var subject = await _supabase.GetSubjectAsync(id, userId.Value);
 if (subject == null) return NotFound();

 subject.IsArchived = !subject.IsArchived;
 subject.UpdatedAt = DateTime.UtcNow;
 await _supabase.UpdateSubjectAsync(subject);

 TempData["Success"] = subject.IsArchived ? "Subject archived." : "Subject restored.";
 return RedirectToAction("Index");
 }

 [HttpPost("{id}/delete")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Delete(Guid id)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 await _supabase.DeleteSubjectAsync(id, userId.Value);
 TempData["Success"] = "Subject deleted.";
 return RedirectToAction("Index");
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
