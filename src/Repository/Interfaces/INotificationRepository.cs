using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Repository.Interfaces
{
    public interface INotificationRepository: IBaseRepository<Notification>
    {
        Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(string userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
    }
}
