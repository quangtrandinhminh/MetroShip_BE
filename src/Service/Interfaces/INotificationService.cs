using MetroShip.Service.ApiModels.Notification;
using MetroShip.Service.ApiModels.PaginatedList;

namespace MetroShip.Service.Interfaces;

public interface INotificationService
{
    Task<PaginatedListResponse<NotificationDto>> GetNotificationsByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<NotificationDto> GetNotificationByIdAsync(int id);
    Task<NotificationDto> CreateNotificationAsync(NotificationCreateRequest request);
    Task<int> UpdateNotificationAsync(NotificationUpdateRequest request);
    Task<int> DeleteNotificationAsync(int id);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<bool> SendNotificationToAllUsersAsync(string message, string title = "Thông báo mới");
}