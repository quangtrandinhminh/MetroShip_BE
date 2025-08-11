using MetroShip.Service.ApiModels.Notification;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace MetroShip.WebAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public static readonly Dictionary<int, string> _userConnectionMap = new Dictionary<int, string>();
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;

        public NotificationHub(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger>();
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var userId = JwtClaimUltils.GetUserId(user);

            _userConnectionMap[userId] = Context.ConnectionId;
            _logger.Information("Người dùng {UserId} đã kết nối với NotificationHub, ConnectionId: {ConnectionId}",
                               userId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = Context.User;
            var userId = JwtClaimUltils.GetUserId(user);

            _userConnectionMap.Remove(userId);
            _logger.Information("Người dùng {UserId} đã ngắt kết nối khỏi NotificationHub", userId);

            if (exception != null)
            {
                _logger.Error(exception, "Lỗi khi ngắt kết nối của người dùng {UserId}", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinNotificationGroup()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var groupName = $"user_{userId}_notifications";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.Information("Người dùng {UserId} đã tham gia nhóm thông báo {GroupName}",
                                  userId, groupName);
            }
        }

        public async Task SendNotification(NotificationDto notification)
        {
            // Gửi thông báo đến người nhận nếu họ đang online
            if (notification.UserId.HasValue && _userConnectionMap.TryGetValue(notification.UserId.Value, out var connectionId))
            {
                _logger.Information("Gửi thông báo SignalR đến người dùng {UserId}, NotificationId: {NotificationId}, Message: {Message}",
                    notification.UserId.Value, notification.NotificationId, notification.Message);

                await Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);

                // Gửi theo cả nhóm nếu người dùng đã tham gia nhóm
                var groupName = $"user_{notification.UserId.Value}_notifications";
                await Clients.Group(groupName).SendAsync("ReceiveNotification", notification);

                _logger.Information("Đã gửi thông báo SignalR đến nhóm {GroupName}", groupName);
            }
            else if (notification.UserId.HasValue)
            {
                _logger.Warning("Không thể gửi thông báo SignalR đến người dùng {UserId} vì họ không online",
                    notification.UserId.Value);
            }
        }

        public async Task ConfirmNotificationReceived(int notificationId)
        {
            try
            {
                _logger.Information("Xác nhận đã nhận thông báo {NotificationId}", notificationId);

                // Đánh dấu thông báo là đã đọc
                var result = await _notificationService.MarkAsReadAsync(notificationId);

                if (result)
                {
                    _logger.Information("Đã đánh dấu thông báo {NotificationId} là đã đọc", notificationId);
                }
                else
                {
                    _logger.Warning("Không thể đánh dấu thông báo {NotificationId} là đã đọc", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xác nhận đã nhận thông báo {NotificationId}", notificationId);
            }
        }
    }
}