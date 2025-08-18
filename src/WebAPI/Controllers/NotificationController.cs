using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.Notification;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Utils;
using MetroShip.Utility.Constants;
using MetroShip.WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IUserService _userService;


        public NotificationController(IServiceProvider serviceProvider)
        {
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            _notificationHubContext = serviceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            _logger = serviceProvider.GetRequiredService<ILogger>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
        }

        [HttpGet]
        [Route(WebApiEndpoint.Notification.GetNotifications)]
        [Authorize]
        public async Task<IActionResult> GetNotifications([FromQuery] PaginatedListRequest request)
        {
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(request.PageNumber, request.PageSize);
            return Ok(BaseResponse.OkResponseDto(notifications));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Notification.GetNotification)]
        public async Task<IActionResult> GetNotification([FromRoute] int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            return Ok(BaseResponse.OkResponseDto(notification));
        }

        [HttpPost]
        [Route(WebApiEndpoint.Notification.CreateNotification)]
        [Authorize]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateRequest request)
        {
            _logger.Information("Đang tạo thông báo mới cho người dùng {UserId}", request.UserId);
            var notification = await _notificationService.CreateNotificationAsync(request);

            // Thông báo thành công tạo thông báo
            _logger.Information("New notification created: NotificationId={NotificationId}, UserId={UserId}, Message={Message}",
                            notification.NotificationId, notification.UserId, notification.Message);

            await SendNotificationViaSignalR(notification);
            

            return Ok(BaseResponse.OkResponseDto(notification));
        }

        

        [HttpPost]
        [Route(WebApiEndpoint.Notification.SendNotificationToAllUsers)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotificationToAllUsers([FromBody] SendNotificationToAllRequest request)
        {
            _logger.Information("Đang gửi thông báo quảng bá đến tất cả người dùng. Title: {Title}, Message: {Message}",
                            request.Title, request.Message);

            var result = await _notificationService.SendNotificationToAllUsersAsync(request.Message, request.Title);

            // Thông báo qua SignalR cho tất cả người dùng
            _logger.Information("Đang gửi thông báo quảng bá qua SignalR");

            await _notificationHubContext.Clients.All.SendAsync("ReceiveBroadcastNotification", new
            {
                Title = request.Title,
                Message = request.Message,
                Timestamp = DateTime.UtcNow.AddHours(7)
            });

            _logger.Information("Đã gửi thông báo quảng bá qua SignalR thành công");

            // Gửi Firebase Push Notification đến tất cả người dùng (theo chủ đề)
            try
            {
                // Chuẩn bị dữ liệu
                var data = new Dictionary<string, string>
                {
                    { "type", "broadcast" },
                    { "timestamp", DateTime.UtcNow.AddHours(7).ToString("o") }
                };

                _logger.Information("Đang gửi thông báo quảng bá qua Firebase đến chủ đề 'all-users'");

            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không ngăn việc trả về kết quả
                _logger.Error(ex, "Lỗi khi gửi Firebase broadcast notification: {ErrorMessage}", ex.Message);
            }

            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPut]
        [Route(WebApiEndpoint.Notification.UpdateNotification)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNotification([FromBody] NotificationUpdateRequest request)
        {
            var result = await _notificationService.UpdateNotificationAsync(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpDelete]
        [Route(WebApiEndpoint.Notification.DeleteNotification)]
        public async Task<IActionResult> DeleteNotification([FromRoute] int id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Notification.GetUnreadCount)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);

            var count = await _notificationService.GetUnreadCountAsync(currentUserId);
            return Ok(BaseResponse.OkResponseDto(count));
        }

        [HttpPut]
        [Route(WebApiEndpoint.Notification.MarkAsRead)]
        public async Task<IActionResult> MarkAsRead([FromRoute] int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPut]
        [Route(WebApiEndpoint.Notification.MarkAllAsRead)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);

            var result = await _notificationService.MarkAllAsReadAsync(currentUserId);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        
        /// <summary>
        /// Gửi thông báo qua SignalR
        /// </summary>
        private async Task SendNotificationViaSignalR(NotificationDto notification)
        {
            try
            {
                if (notification.UserId.HasValue)
                {
                    _logger.Information("Đang gửi thông báo SignalR cho người dùng {UserId}, NotificationId: {NotificationId}",
                                    notification.UserId.Value, notification.NotificationId);

                    // Gửi thông báo qua SignalR sử dụng connectionId từ mapping
                    if (NotificationHub._userConnectionMap.TryGetValue(notification.UserId.Value, out var connectionId))
                    {
                        _logger.Information("Người dùng {UserId} đang kết nối với SignalR, gửi thông báo qua connectionId {ConnectionId}",
                                        notification.UserId.Value, connectionId);

                        await _notificationHubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);

                        _logger.Information("Đã gửi thông báo SignalR đến connectionId {ConnectionId} thành công", connectionId);
                    }
                    else
                    {
                        _logger.Warning("Người dùng {UserId} không kết nối với NotificationHub, không thể gửi qua connectionId",
                                    notification.UserId.Value);
                    }

                    // Gửi thông báo đến nhóm người dùng
                    var groupName = $"user_{notification.UserId.Value}_notifications";
                    _logger.Information("Đang gửi thông báo SignalR đến nhóm {GroupName}", groupName);

                    await _notificationHubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);

                    _logger.Information("Đã gửi thông báo SignalR đến nhóm {GroupName} thành công", groupName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi gửi thông báo qua SignalR: {ErrorMessage}", ex.Message);
            }
        }
    }
}
