using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Repository.Repositories
{
    public class NotificationRepository: BaseRepository<Notification> , INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            var userIdString = userId;
            var query = _context.Notifications
                .Where(n => n.ToUserId == userIdString)
                .OrderByDescending(n => n.CreatedAt)
                .Include(n => n.User);

            return await PaginatedList<Notification>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var userIdString = userId.ToString();
            return await _context.Notifications
                .CountAsync(n => n.ToUserId == userIdString && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            var notification = await _context.Notifications
                .AsTracking()
                .FirstOrDefaultAsync(n => n.Id == notificationId.ToString());

            if (notification == null) return false;

            notification.IsRead = true;

            _context.Entry(notification).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var userIdString = userId.ToString();
            var unreadNotifications = await _context.Notifications
                .AsTracking()
                .Where(n => n.ToUserId == userIdString && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Count == 0) return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                _context.Entry(notification).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
