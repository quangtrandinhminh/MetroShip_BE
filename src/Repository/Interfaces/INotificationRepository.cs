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
        Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(int userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
    }
}
