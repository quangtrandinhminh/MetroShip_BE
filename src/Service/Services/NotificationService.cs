using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Notification;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using RestSharp.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Services
{
    public class NotificationService: INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger _logger;
        private readonly IMapperlyMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger logger,
            IMapperlyMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public async Task<PaginatedListResponse<NotificationDto>> GetNotificationsByUserIdAsync(int pageNumber, int pageSize)
        {
            var currentUserId = JwtClaimUltils.GetUserId(_httpContextAccessor);

            _logger.Information($"Getting notifications for user {currentUserId}, page {pageNumber}, size {pageSize}");
            var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(
                currentUserId,
                pageNumber, 
                pageSize
            );
            return _mapper.MapNotificationList(notifications);
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(string id)
        {
            _logger.Information($"Getting notification with id {id}");
            var notification = await _notificationRepository.GetSingleAsync(x => x.Id == id.ToString());
            if (notification == null)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                    "Không tìm thấy thông báo", StatusCodes.Status404NotFound);
            }

            return _mapper.MapNotification(notification);
        }

        public async Task<NotificationDto> CreateNotificationAsync(NotificationCreateRequest request)
        {
            _logger.Information($"Creating notification for user {request.ToUserId}");

            var notification = _mapper.MapNotificationRequest(request);
            notification.CreatedAt = DateTime.UtcNow.AddHours(7);
            notification.IsRead = false;

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            var result = _mapper.MapNotification(notification);

            // Gửi thông báo đ PUSH qua Firebase
            //await SendPushNotificationAsync(notification);

            return result;
        }

        /*private async Task SendPushNotificationAsync(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.ToUserId))
                return;

            
            var dto = _mapper.MapNotification(notification);
            dto.SendAt = DateTimeOffset.UtcNow.AddHours(7);

            
            await Task.CompletedTask;
        }*/

        public async Task<int> UpdateNotificationAsync(NotificationUpdateRequest request)
        {
            _logger.Information($"Updating notification {request.NotificationId}");

            var notification = await _notificationRepository.GetSingleAsync(x => x.Id == request.NotificationId.ToString());
            if (notification == null)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                    "Không tìm thấy thông báo", StatusCodes.Status404NotFound);
            }

            // Kiểm tra quyền chỉnh sửa (chỉ admin mới có quyền)
            //var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var roles = JwtClaimUltils.GetUserRole(_httpContextAccessor);

            if (!roles.Contains("Admin"))
            {
                throw new AppException(HttpResponseCodeConstants.FORBIDDEN,
                    "Bạn không có quyền chỉnh sửa thông báo này", StatusCodes.Status403Forbidden);
            }

            _mapper.MapNotificationUpdate(request, notification);
            _notificationRepository.Update(notification);

            await _notificationRepository.SaveChangesAsync();
            return 1;
        }

        public async Task<int> DeleteNotificationAsync(string id)
        {
            _logger.Information($"Deleting notification {id}");

            var notification = await _notificationRepository.GetSingleAsync(x => x.Id == id.ToString());
            if (notification == null)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                    "Không tìm thấy thông báo", StatusCodes.Status404NotFound);
            }

            // Kiểm tra quyền xóa (chỉ admin hoặc chủ sở hữu mới có quyền)
            //var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(_httpContextAccessor);
            var roles = JwtClaimUltils.GetUserRole(_httpContextAccessor);

            if (notification.ToUserId != currentUserId && !roles.Contains(nameof(UserRoleEnum.Admin)))
            {
                throw new AppException(HttpResponseCodeConstants.FORBIDDEN,
                    "Bạn không có quyền xóa thông báo này", StatusCodes.Status403Forbidden);
            }

            _notificationRepository.Delete(notification);

           await _notificationRepository.SaveChangesAsync();
            return 1;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            _logger.Information($"Getting unread notification count for user {userId}");
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            _logger.Information($"Marking notification {notificationId} as read");
            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            _logger.Information($"Marking all notifications as read for user {userId}");
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task<bool> SendNotificationToAllUsersAsync(string message, string title = "Thông báo mới")
        {
            _logger.Information("Sending notification to all users: {Message}", message);

            try
            {
                // Lấy toàn bộ user (phân trang tối đa)
                var usersPaginated = await _userRepository.GetAllPaginatedQueryable(1, int.MaxValue);
                var users = usersPaginated.Items;
                var successCount = 0;

                foreach (var user in users)
                {
                    // Tạo thông báo cho mỗi người dùng
                    var notificationRequest = new NotificationCreateRequest
                    {
                        ToUserId = user.Id,
                        Message = message
                    };

                    try
                    {
                        await CreateNotificationAsync(notificationRequest);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error creating notification for user {UserId}", user.Id);
                    }
                }

                _logger.Information(
                    "Successfully created notifications for {SuccessCount}/{TotalCount} users",
                    successCount,
                    users.Count
                );

                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error sending notification to all users");
                return false;
            }
        }
    }
}
