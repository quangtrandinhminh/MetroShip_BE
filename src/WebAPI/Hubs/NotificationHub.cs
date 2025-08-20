using MetroShip.Service.ApiModels.Notification;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RestSharp.Extensions;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace MetroShip.WebAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public static readonly Dictionary<string, string> _userConnectionMap = new Dictionary<string, string>();
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationHub(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger>();
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        }

        public override async Task OnConnectedAsync()
        {
            var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);

            _userConnectionMap[userId] = Context.ConnectionId;
            _logger.Information("User {UserId} connected to NotificationHub, ConnectionId: {ConnectionId}",
                               userId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);

            _userConnectionMap.Remove(userId);
            _logger.Information("User {UserId} disconnected to NotificationHub", userId);

            if (exception != null)
            {
                _logger.Error(exception, "Error when disconnecting for user {UserId}", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinNotificationGroup()
        {
            var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);
            if (!string.IsNullOrEmpty(userId))
            {
                var groupName = $"user_{userId}_notifications";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.Information("User {UserId} joined notification group {GroupName}",
                                  userId, groupName);
            }
        }

        public async Task SendNotification(NotificationDto notification)
        {
            // Gửi thông báo đến người nhận nếu họ đang online
            if (notification.ToUserId.HasValue() && _userConnectionMap.TryGetValue(notification.ToUserId, out var connectionId))
            {
                _logger.Information("Sending SignalR notification to user {UserId}, NotificationId: {NotificationId}, Message: {Message}",
                    notification.ToUserId, notification.Id, notification.Message);

                await Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);

                // Gửi theo cả nhóm nếu người dùng đã tham gia nhóm
                var groupName = $"user_{notification.ToUserId}_notifications";
                await Clients.Group(groupName).SendAsync("ReceiveNotification", notification);

                _logger.Information("Sent SignalR notification to group {GroupName}", groupName);
            }
            else if (notification.ToUserId.HasValue())
            {
                _logger.Warning("Cannot send notification to user {UserId} because they are not connected",
                notification.ToUserId);
            }
        }

        public async Task ConfirmNotificationReceived(string notificationId)
        {
            try
            {
                _logger.Information("Confirm received {NotificationId}", notificationId);

                // Đánh dấu thông báo là đã đọc
                var result = await _notificationService.MarkAsReadAsync(notificationId);

                if (result)
                {
                    _logger.Information("Marked notification {NotificationId} to Read", notificationId);
                }
                else
                {
                    _logger.Warning("Cannot Marked notification {NotificationId} to Read", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error when receiving notification {NotificationId}", notificationId);
            }
        }
    }
}