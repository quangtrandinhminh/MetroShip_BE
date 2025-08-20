using MetroShip.Service.ApiModels.Notification;
using MetroShip.Service.ApiModels.PaginatedList;

namespace MetroShip.Service.Interfaces;

public interface INotificationService
{
    Task<PaginatedListResponse<NotificationDto>> GetNotificationsByUserIdAsync( int pageNumber, int pageSize);
    Task<NotificationDto> GetNotificationByIdAsync(string id);
    Task<NotificationDto> CreateNotificationAsync(NotificationCreateRequest request);
    Task<int> UpdateNotificationAsync(NotificationUpdateRequest request);
    Task<int> DeleteNotificationAsync(string id);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> SendNotificationToAllUsersAsync(string message, string title = "Thông báo mới");
}