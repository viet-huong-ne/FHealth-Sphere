using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract.Repositories.Entity;

namespace Contract.Services.Interface
{

    public interface INotificationService
    {
        Task<NotificationSystem> CreateNotification(string title, string message, int? accountId);
        Task<IEnumerable<NotificationSystem>> GetNotifications(int accountId);
        Task MarkAsRead(int notificationId);
    }

}
