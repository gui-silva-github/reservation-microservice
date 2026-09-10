using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.NotificationsService.API.DTOs;
using Reservation.NotificationsService.BLL.DTOs;
using Reservation.NotificationsService.BLL.Interfaces;

namespace Reservation.NotificationsService.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [HttpPost("send")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            NotificationResponse notification = await _notificationService.SendAsync(request);
            return CreatedAtAction(nameof(GetByUserId), new { userId = notification.UserId }, notification);
        }

        [HttpGet("userid/{userId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<NotificationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            Guid? authenticatedUserId = GetAuthenticatedUserId();
            string? role = User.FindFirst(ClaimTypes.Role)?.Value;

            IReadOnlyList<NotificationResponse> notifications =
                await _notificationService.GetByUserIdAsync(userId, authenticatedUserId, role);

            return Ok(notifications);
        }

        [HttpPatch("{id:guid}/read")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            NotificationResponse notification = await _notificationService.MarkAsReadAsync(
                id,
                GetAuthenticatedUserId(),
                User.FindFirst(ClaimTypes.Role)?.Value);

            return Ok(notification);
        }

        [HttpPatch("userid/{userId:guid}/read-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkAllAsRead(Guid userId)
        {
            await _notificationService.MarkAllAsReadAsync(
                userId,
                GetAuthenticatedUserId(),
                User.FindFirst(ClaimTypes.Role)?.Value);

            return NoContent();
        }

        private Guid? GetAuthenticatedUserId()
        {
            string? userIdClaim = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : null;
        }
    }
}
