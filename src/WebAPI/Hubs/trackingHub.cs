using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Utils;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System.Web.Http;
using ILogger = Serilog.ILogger;

namespace MetroShip.WebAPI.Hubs;

//[Authorize]
public class trackingHub : Hub
{
    public static readonly Dictionary<int, string> _userConnectionMap = new();
    private readonly ILogger _logger;
    private readonly ITrainService _trainService;

    public trackingHub(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILogger>();
        _trainService = serviceProvider.GetRequiredService<ITrainService>();
    }

    /*public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var userId = JwtClaimUltils.GetUserId(user);

        _userConnectionMap[userId] = Context.ConnectionId;

        _logger.Information("Người dùng {UserId} đã kết nối TrackingHub với ConnectionId {ConnectionId}",
            userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User;
        var userId = JwtClaimUltils.GetUserId(user);

        _userConnectionMap.Remove(userId);

        _logger.Information("Người dùng {UserId} đã ngắt kết nối khỏi TrackingHub", userId);

        if (exception != null)
        {
            _logger.Error(exception, "Lỗi khi ngắt kết nối người dùng {UserId}", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }*/

    public async Task JoinTrackingRoom(string trackingCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, trackingCode);
        _logger.Information("Connection {ConnectionId} đã tham gia room {TrackingCode}",
        Context.ConnectionId, trackingCode);
    }

    public async Task LeaveTrackingRoom(string trackingCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, trackingCode);
        _logger.Information("Connection {ConnectionId} đã rời khỏi room {TrackingCode}",
            Context.ConnectionId, trackingCode);
    }

    // hàm này được gọi bởi invoke signalR từ client (gps của train)
    public async Task SendLocationUpdate(TrackingLocationUpdateDto location)
    {
        /*_logger.Information("📍 Nhận tọa độ mới từ TrainId {TrainId}, TrackingCode: {TrackingCode} | ({Lat}, {Lng})",
            location.TrainId, location.TrackingCode, location.Latitude, location.Longitude);*/

        // 1. Kiểm tra xem đơn hàng đã giao chưa
        /*bool isDelivered = await _trainService.IsShipmentDeliveredAsync(location.TrackingCode);
        if (isDelivered)
        {
            _logger.Warning("🚫 Shipment {TrackingCode} đã giao hàng. Ngắt kết nối room SignalR.", location.TrackingCode);

            await Clients.Group(location.TrackingCode).SendAsync("ShipmentDelivered", new
            {
                TrackingCode = location.TrackingCode,
                Message = "Hàng đã được giao thành công. Ngắt tracking."
            });

            return;
        }*/

        // 2. Gửi thông tin tọa độ đến tất cả client đang theo dõi shipment này
        await Clients.Group(location.TrainId).SendAsync("ReceiveLocationUpdate", location);
    }

    public async Task ConfirmLocationReceived(string trackingCode)
    {
        var userId = JwtClaimUltils.GetUserId(Context.User);
        _logger.Information("✅ User {UserId} xác nhận đã nhận location của {TrackingCode}", userId, trackingCode);
        await Task.CompletedTask;
    }

    public Task<int> GetActiveConnectionCount()
    {
        return Task.FromResult(_userConnectionMap.Count);
    }
}