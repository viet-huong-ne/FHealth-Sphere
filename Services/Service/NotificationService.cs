using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NotificationSystem> CreateNotification(string title, string message, int? accountId)
    {
        var notification = new NotificationSystem
        {
            AccountId = accountId,
            Content = $"{title}: {message}",
            status = "Unread",
            CreatedTime = DateTimeOffset.Now
        };

        await _unitOfWork.GetRepository<NotificationSystem>().InsertAsync(notification);
        await _unitOfWork.SaveAsync();

        // Create NotificationWatcher entries for all watchers of the account
        var watchers = await _unitOfWork.GetRepository<Watcher>()
            .Entities
            .Where(w => w.PatientId == accountId)
            .ToListAsync();

        foreach (var watcher in watchers)
        {
            var notificationWatcher = new NotificationWatcher
            {
                WatcherId = watcher.Id,
                Status = false,
                Time = DateTime.Now,
                NotificationSystem = notification
            };

            await _unitOfWork.GetRepository<NotificationWatcher>().InsertAsync(notificationWatcher);
        }

        await _unitOfWork.SaveAsync();

        return notification;
    }

    public async Task<IEnumerable<NotificationSystem>> GetNotifications(int accountId)
    {
        return await _unitOfWork.GetRepository<NotificationSystem>()
            .Entities
            .Where(n => n.AccountId == accountId)
            .ToListAsync();
    }

    public async Task MarkAsRead(int notificationId)
    {
        var notification = await _unitOfWork.GetRepository<NotificationSystem>().GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.status = "Read";
            await _unitOfWork.GetRepository<NotificationSystem>().UpdateAsync(notification);
            await _unitOfWork.SaveAsync();
        }
    }
}
