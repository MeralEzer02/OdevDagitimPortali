using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ÖdevDağıtım.API.DTOs;
using ÖdevDağıtım.API.Services;

namespace ÖdevDağıtım.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications([FromQuery] PaginationParams paginationParams)
        {
            var userId = _currentUserService.UserId;
            var result = await _notificationService.GetUserNotificationsAsync(userId!, paginationParams);
            return Ok(result);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }
    }
}