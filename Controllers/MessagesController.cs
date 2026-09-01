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
        var existingOtherUserIds = new HashSet<Guid>();

        foreach (var conv in conversations)
        {
            var otherId = conv.User1Id == userId.Value ? conv.User2Id : conv.User1Id;
            existingOtherUserIds.Add(otherId);
            var otherUser = await _supabase.GetProfileByIdAsync(otherId);
            var messages = await _supabase.GetMessagesAsync(conv.Id);
            var unread = messages.Count(m => m.ReadAt == null && m.SenderId != userId.Value);

            viewModels.Add(new ConversationViewModel
            {
                ConversationId = conv.Id,
                OtherUserId = otherId,
                OtherUserName = otherUser?.FullName ?? "Student",
                OtherUserImage = otherUser?.ProfileImageUrl,
                LastMessage = messages.LastOrDefault()?.Content ?? "No messages yet",
                LastMessageAt = messages.LastOrDefault()?.CreatedAt ?? conv.CreatedAt,
                IsOnline = otherUser?.IsOnline ?? false,
                UnreadCount = unread
            });
        }

        viewModels = viewModels.OrderByDescending(v => v.LastMessageAt).ToList();

        // Build list of accepted connections without active conversations for quick start
        var newConnectionsToChat = new List<ConnectionItemViewModel>();
        foreach (var conn in acceptedConnections)
        {
            var otherId = conn.RequesterId == userId.Value ? conn.ReceiverId : conn.RequesterId;
            if (!existingOtherUserIds.Contains(otherId))
            {
                var otherUser = await _supabase.GetProfileByIdAsync(otherId);
                newConnectionsToChat.Add(new ConnectionItemViewModel
                {
                    ConnectionId = conn.Id,
                    OtherUserId = otherId,
                    OtherUserName = otherUser?.FullName ?? "Student",
                    OtherUserImage = otherUser?.ProfileImageUrl,
                    College = otherUser?.College,
                    Course = otherUser?.Course,
                    Station = otherUser?.Station,
                    IsOnline = otherUser?.IsOnline ?? false
                });
            }
        }

        ViewBag.NewConnectionsToChat = newConnectionsToChat;
        return View(viewModels);
    }

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> Chat(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var conv = await _supabase.GetConversationByIdAsync(conversationId);
        if (conv == null || (conv.User1Id != userId.Value && conv.User2Id != userId.Value))
        {
            return RedirectToAction("Index");
        }

        var otherId = conv.User1Id == userId.Value ? conv.User2Id : conv.User1Id;
        var otherUser = await _supabase.GetProfileByIdAsync(otherId);

        var messages = await _supabase.GetMessagesAsync(conversationId);

        // Mark messages as read
        await _supabase.MarkMessagesAsReadAsync(conversationId, userId.Value);

        var vm = new ChatViewModel
        {
            ConversationId = conversationId,
            OtherUserId = otherId,
            OtherUserName = otherUser?.FullName ?? "Student",
            OtherUserImage = otherUser?.ProfileImageUrl,
            IsOnline = otherUser?.IsOnline ?? false,
            Messages = messages.Select(m => new ChatMessageViewModel
            {
                Id = m.Id,
                SenderId = m.SenderId,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                IsMine = m.SenderId == userId.Value,
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

        return RedirectToAction("Chat", new { conversationId });
    }

    [HttpGet("start-chat/{userId}")]
    [HttpPost("start-chat/{userId}")]
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
