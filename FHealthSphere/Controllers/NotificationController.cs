using Microsoft.AspNetCore.Mvc;
using Contract.Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Generic;

[Route("api/[controller]")] // Route sẽ là /api/notifications
[ApiController]
public class NotificationsController : ControllerBase // Đổi tên controller thành số nhiều
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // DTO để nhận dữ liệu từ request
    public class BulkNotificationRequest
    {
        [Required]
        public List<NotificationItem> Notifications { get; set; }
    }

    public class NotificationItem
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public string FcmToken { get; set; }
    }

    [HttpPost] 
    public async Task<IActionResult> CreateBulkNotifications([FromBody] BulkNotificationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var tasks = new List<Task>();

            foreach (var item in request.Notifications)
            {
                // Tạo notification trong database
                var notificationTask = _notificationService.CreateNotification(
                    item.Title,
                    item.Message,
                    item.AccountId);

                // Gửi push notification qua FCM
                var pushTask = _notificationService.SendNotificationAsync(
                    item.FcmToken,
                    item.Title,
                    item.Message);

                tasks.Add(notificationTask);
                tasks.Add(pushTask);
            }

            // Chờ tất cả các tác vụ hoàn thành
            await Task.WhenAll(tasks);

            return Ok(new
            {
                Success = true,
                Message = $"Successfully sent {request.Notifications.Count} notifications",
                Timestamp = DateTimeOffset.Now
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "Failed to send notifications",
                Error = ex.Message
            });
        }
    }
}