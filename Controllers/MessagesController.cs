using Attendly.Data;
using Attendly.Models;
using Attendly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

[Route("messages")]
public class MessagesController : Controller
{
 private readonly ISupabaseService _supabase;

 public MessagesController(ISupabaseService supabase)
 {
 _supabase = supabase;
 }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var conversations = await _supabase.GetUserConversationsAsync(userId.Value);
 var connections = await _supabase.GetUserConnectionsAsync(userId.Value);
 var acceptedConnections = connections.Where(c => c.Status == "accepted").ToList();

 var viewModels = new List<ConversationViewModel>();

 foreach (var conv in conversations)
 {
 var otherId = conv.User1Id == userId ? conv.User2Id : conv.User1Id;
 var otherUser = await _supabase.GetProfileByIdAsync(otherId);
 var messages = await _supabase.GetMessagesAsync(conv.Id);
 var unread = messages.Count(m => m.ReadAt == null && m.SenderId != userId);

 viewModels.Add(new ConversationViewModel
 {
 ConversationId = conv.Id,
 OtherUserId = otherId,
 OtherUserName = otherUser?.FullName ?? "User",
 OtherUserImage = otherUser?.ProfileImageUrl,
 LastMessage = messages.LastOrDefault()?.Content,
 LastMessageAt = messages.LastOrDefault()?.CreatedAt,
 IsOnline = otherUser?.IsOnline ?? false,
 UnreadCount = unread
 });
 }

 viewModels = viewModels.OrderByDescending(v => v.LastMessageAt).ToList();
 ViewBag.AcceptedConnections = acceptedConnections;
 return View(viewModels);
 }

 [HttpGet("{conversationId}")]
 public async Task<IActionResult> Chat(Guid conversationId)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 var conversations = await _supabase.GetUserConversationsAsync(userId.Value);
 var conv = conversations.FirstOrDefault(c => c.Id == conversationId);
 if (conv == null) return NotFound();

 var otherId = conv.User1Id == userId ? conv.User2Id : conv.User1Id;
 var otherUser = await _supabase.GetProfileByIdAsync(otherId);

 var messages = await _supabase.GetMessagesAsync(conversationId);

 await _supabase.MarkMessagesAsReadAsync(conversationId, userId.Value);

 var vm = new ChatViewModel
 {
 ConversationId = conversationId,
 OtherUserId = otherId,
 OtherUserName = otherUser?.FullName ?? "User",
 OtherUserImage = otherUser?.ProfileImageUrl,
 IsOnline = otherUser?.IsOnline ?? false,
 Messages = messages.Select(m => new ChatMessageViewModel
 {
 Id = m.Id,
 SenderId = m.SenderId,
 Content = m.Content,
 CreatedAt = m.CreatedAt,
 IsMine = m.SenderId == userId,
 IsRead = m.ReadAt != null
 }).ToList()
 };

 return View(vm);
 }

 [HttpPost("{conversationId}/send")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Send(Guid conversationId, string message)
 {
 var userId = GetCurrentUserId();
 if (userId == null) return RedirectToAction("Login", "Account");

 if (string.IsNullOrWhiteSpace(message))
 return RedirectToAction("Chat", new { conversationId });

 var msg = new Message
 {
 ConversationId = conversationId,
 SenderId = userId.Value,
 Content = message.Trim(),
 CreatedAt = DateTime.UtcNow
 };

 await _supabase.CreateMessageAsync(msg);

 // Update other user's last_seen
 var conversations = await _supabase.GetUserConversationsAsync(userId.Value);
 var conv = conversations.FirstOrDefault(c => c.Id == conversationId);
 if (conv != null)
 {
 var otherId = conv.User1Id == userId.Value ? conv.User2Id : conv.User1Id;
 await _supabase.UpdateProfileAsync(new StudentProfile { Id = otherId, LastSeen = DateTime.UtcNow });
 }

 return RedirectToAction("Chat", new { conversationId });
 }

 [HttpPost("start-chat/{userId}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> StartChat(Guid userId)
 {
 var currentUserId = GetCurrentUserId();
 if (currentUserId == null) return RedirectToAction("Login", "Account");

 var conv = await _supabase.GetConversationAsync(currentUserId.Value, userId);
 if (conv == null)
 {
 conv = await _supabase.CreateConversationAsync(currentUserId.Value, userId);
 }

 return RedirectToAction("Chat", new { conversationId = conv.Id });
 }

 private Guid? GetCurrentUserId()
 {
 var id = HttpContext.Session.GetString("UserId");
 if (Guid.TryParse(id, out var guid)) return guid;
 return null;
 }
}
