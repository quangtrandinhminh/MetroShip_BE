using AngleSharp.Dom;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.WebAPI.Hubs;
using MetroShip.WebAPI.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using ILogger = Serilog.ILogger;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class TrainController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ITrainService _trainService = serviceProvider.GetRequiredService<ITrainService>();
        private readonly IHubContext<TrackingHub> _hub = serviceProvider.GetRequiredService<IHubContext<TrackingHub>>();
        private readonly ISchedulerFactory _schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

        //[Authorize]
        [HttpGet]
        [Route(WebApiEndpoint.MetroTrainEndpoint.GetAllTrains)]
        [ProducesResponseType(typeof(BaseResponse<PaginatedListResponse<TrainListResponse>>), 200)]
        public async Task<IActionResult> GetTrainsAsync([FromQuery] TrainListFilterRequest request)
        {
            var response = await _trainService.PaginatedListResponse(request);
            var additionalData = await _trainService.GetTrainSystemConfigAsync();
            return Ok(BaseResponse.OkResponseDto(response, additionalData));
        }

        [HttpPost(WebApiEndpoint.MetroTrainEndpoint.CreateTrain)]
        public async Task<IActionResult> CreateTrainAsync([FromBody] CreateTrainRequest request)
        {
            var response = await _trainService.CreateTrainAsync(request);
            return Ok(BaseResponse.OkResponseDto(response, null));
        }

        //[Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPost(WebApiEndpoint.MetroTrainEndpoint.SendLocation)]
        public async Task<IActionResult> SendLocation([FromBody] TrackingLocationUpdateDto location)
        {
            Console.WriteLine($"Received location update for train " +
                $"{location.TrainId}: Lat {location.Latitude}, Lng {location.Longitude}, StationId {location.StationId}");
            // Update db quá nhiều, không kịp gửi API và chạy transaction trong khoảng 3-5s
            /*await _trainService.UpdateTrainLocationAsync(location.TrainId, location.Latitude, location.Longitude, location.StationId);
            var trackingCodes = await _trainService.GetTrackingCodesByTrainAsync(location.TrainId);

            foreach (var code in trackingCodes)
            {
                // Số lượng tracking codes quá lớn gây ra độ trễ cao
            }*/

            // xử lý tại service
            // Khi 1 đơn được yêu cầu, trả về trainId trong itinerary đầu tiên chưa completed (orderBy legOrder)
            // Client sẽ connect room theo trainId, sau đó sẽ nhận location update từ server
            // Đơn nào intransit mới được connect room,
            // nếu staff chưa nhận hàng -> không cần gửi location,
            // Nếu metro đã nhận hàng tại trạm nào -> gửi current station location
            // Yêu cầu train gps gửi vị trí qua request gồm: trainId, timeSlotId, timestamp, latitude, longitude
            // stationId: nullable,  routeId: nullable, khi nào tới trạm thì gps gửi
            // Khi train gửi location, hub send to clent, có thể lưu realtime mongo nếu cần, không cũng được
            // Khi stationId & routeId được gửi: 
            // - Update itinerary theo trainId, timeSlotId, routeId => IsCompleted true + include shipment currentStationId
            // - Update train coordinate vào sql
            // - Nếu current stationId trùng với destination stationId của shipment thì đánh dấu shipment đã giao hàng
            // - Điều kiện check shipment trung chuyển: this itinerary.Route.LineId (completed) != itinerary.Route.LineId (next, not completed), orderBy legOrder

            await _hub.Clients.Group(location.TrainId).SendAsync("ReceiveLocationUpdate", location);
            return Ok();
        }

        [Authorize]
        [HttpGet(WebApiEndpoint.MetroTrainEndpoint.GetTrainsByLineSlotAndDate)]
        public async Task<IActionResult> GetAllTrainsByLineSlotDateAsync(LineSlotDateFilterRequest request)
        {
            var response = await _trainService.GetAllTrainsByLineSlotDateAsync(request);
            var additionalData = await _trainService.GetTrainSystemConfigAsync();
            return Ok(BaseResponse.OkResponseDto(response, additionalData));
        }

        [Authorize]
        [HttpPost(WebApiEndpoint.MetroTrainEndpoint.AddShipmentItinerariesForTrain)]
        public async Task<IActionResult> AddShipmentItinerariesForTrainAsync(
            [FromBody] AddTrainToItinerariesRequest request)
        {
            var response = await _trainService.AddShipmentItinerariesForTrain(request);
            return Ok(BaseResponse.OkResponseDto(response, null));
        }

        /// <summary>
        /// for test first
        /// </summary>
        /// <param name="trackingCode"></param>
        /// <returns></returns>
        [HttpGet("api/{trackingCode}/position")]
        public async Task<IActionResult> GetPositionByTrackingCode(string trackingCode)
        {
            var result = await _trainService.GetTrainPositionByTrackingCodeAsync(trackingCode);
            return Ok(result);
        }

        //[Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpGet("/api/train/{trainId}/position")]
        public async Task<IActionResult> GetPositionByTrainId(string trainId)
        {
            var result = await _trainService.GetTrainPositionAsync(trainId);

            // Gắn thêm dòng này là sẽ real-time không cần fetch mỗi 5s, chỉ cần connect với hub, khi nào hub trigged thì gọi api này
            //await _hub.Clients.Group(trainId).SendAsync("ReceiveLocationUpdate", result);
            return Ok(result);
        }

        //[Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPost("/api/train/{trainId}/status")]
        public async Task<IActionResult> UpdateTrainStatus(string trainId)
        {
            await _trainService.StartOrContinueSimulationAsync(trainId);
            return Ok(BaseResponse.OkResponseDto("Train status updated successfully", null));
        }

        [HttpPost("/api/train/{trainId}/confirm-arrival")]
        public async Task<IActionResult> ConfirmTrainArrived(string trainId, [FromQuery] string stationId)
        {
            await _trainService.ConfirmTrainArrivedAsync(trainId, stationId);
            return Ok(BaseResponse.OkResponseDto("Train arrival confirmed successfully", null));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff), Policy = $"RequireAssignmentRole.{nameof(AssignmentRoleEnum.TrainOperator)}")]
        [HttpPost("/api/train/confirm-arrival/{nextStationId}")]
        public async Task<IActionResult> ConfirmTrainArrived([FromRoute] string nextStationId)
        {
            await _trainService.ConfirmTrainArrivedAsync(nextStationId);
            return Ok(BaseResponse.OkResponseDto("Đã xác nhận tàu đến trạm.", null));
        }

        [HttpPost("/api/train/schedule")]
        public async Task<IActionResult> ScheduleTrainAsync([FromForm] string trainIdOrCode, [FromQuery(Name = "startFromEnd")] int startFromEndValue = 0)
        {
            // ép về bool: 1 => true, còn lại => false
            bool startFromEnd = startFromEndValue == 1;

            var response = await _trainService.ScheduleTrainAsync(trainIdOrCode, startFromEnd);
            return Ok(BaseResponse.OkResponseDto(response, null));
        }

        // for test
        [HttpGet("{trainId}/position")]
        public async Task<IActionResult> GetTrainPosition(string trainId)
        {
                var result = await _trainService.GetTrainPositionAsync1(trainId);
                return Ok(BaseResponse.OkResponseDto(result));
            
        }

        /// <summary>
        /// Lấy thêm dữ liệu bổ sung (FullPath, Shipments, Parcels).
        /// </summary>
        [HttpGet("{trainId}/additional")]
        public async Task<IActionResult> GetTrainAdditionalData(string trainId)
        {
                var result = await _trainService.GetTrainAdditionalDataAsync(trainId);
                return Ok(BaseResponse.OkResponseDto(result));
            
        }

        /// <summary>
        /// Gửi cập nhật vị trí đến tất cả client đang theo dõi qua SignalR Hub.
        /// </summary>
        [HttpPost("{trainId}/broadcast")]
        public async Task<IActionResult> BroadcastTrainPosition(string trainId)
        {
            try
            {
                var result = await _trainService.GetTrainPositionAsync1(trainId);

                // Phát đi room có tên = trainId
                await _hub.Clients.Group(trainId).SendAsync("ReceiveLocationUpdate", result);


                return Ok(new { Message = "Broadcast thành công", TrainId = trainId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        // call by start train simulation
        private async Task ScheduleSimulateTrainJob(string trainId, int intervalInSeconds)
        {
            _logger.Information("Scheduling simulator train job for train ID: {TrainId}", trainId);
            var jobData = new JobDataMap
            {
                { "Simulate-for-trainId", trainId }
            };

            var jobDetail = JobBuilder.Create<SimulateTrainJob>()
                .WithIdentity($"SimulatorTrainJob-{trainId}")
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"Trigger-SimulatorTrainJob-{trainId}")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(intervalInSeconds)
                    .RepeatForever())
                .Build();

            await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
        }
    }
}
