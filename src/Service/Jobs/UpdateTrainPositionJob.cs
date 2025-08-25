using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Enums;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;

namespace MetroShip.Service.Jobs
{
    [DisallowConcurrentExecution] // ✅ tránh chạy song song 2 instance
    public class UpdateTrainAndShipmentJob(IServiceProvider serviceProvider) : IJob
    {
        private readonly ITrainRepository _trainRepository = serviceProvider.GetRequiredService<ITrainRepository>();
        private readonly ITrainStateStoreService _trainStateStore = serviceProvider.GetRequiredService<ITrainStateStoreService>();
        private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();
        private readonly IShipmentItineraryRepository _shipmentItineraryRepository = serviceProvider.GetRequiredService<IShipmentItineraryRepository>();
        private readonly IShipmentTrackingRepository _shipmentTrackingRepository = serviceProvider.GetRequiredService<IShipmentTrackingRepository>();
        private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

        public async Task Execute(IJobExecutionContext context)
        {
            var activeTrainIds = await _trainStateStore.GetAllActiveTrainIdsAsync();
            if (activeTrainIds.Count == 0)
            {
                _logger.Information("⏳ No active trains found for tracking update.");
                return;
            }

            foreach (var trainId in activeTrainIds)
            {
                try
                {
                    await UpdateTrainAndShipments(trainId);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "❌ Error updating train/shipment tracking for {TrainId}", trainId);
                }
            }
        }

        private async Task UpdateTrainAndShipments(string trainId)
        {
            var direction = await _trainStateStore.GetDirectionAsync(trainId);
            var segmentIndex = await _trainStateStore.GetSegmentIndexAsync(trainId);
            var startTime = await _trainStateStore.GetStartTimeAsync(trainId);

            if (direction is null || segmentIndex is null || startTime is null)
            {
                _logger.Warning("⚠️ Train {TrainId} missing state in Firebase.", trainId);
                return;
            }

            var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction.Value);
            if (train is null)
            {
                _logger.Warning("⚠️ Train {TrainId} not found in DB.", trainId);
                return;
            }

            // --- Tính toán vị trí train ---
            var routes = train.Line!.Routes
                .Where(r => r.Direction == direction)
                .OrderBy(r => r.SeqOrder)
                .ToList();

            var routePolylines = routes.Select(r => new RoutePolylineDto
            {
                FromStation = r.FromStation?.StationNameVi ?? "",
                ToStation = r.ToStation?.StationNameVi ?? "",
                SeqOrder = r.SeqOrder,
                Direction = r.Direction,
                Polyline = new List<GeoPoint>() // TODO: generate polyline nếu cần
            }).ToList();

            var position = TrainPositionCalculator.CalculatePosition(
                train,
                routePolylines,
                startTime.Value,
                segmentIndex.Value,
                direction.Value,
                train.TopSpeedKmH ?? 100);

            // Lưu vị trí tàu vào Firebase
            await _trainStateStore.SetPositionResultAsync(trainId, position);

            _logger.Information("🚆 Train {TrainId} updated: {From}->{To}, Progress {Progress}% @ {Lat},{Lng}",
                trainId, position.FromStation, position.ToStation,
                position.ProgressPercent, position.Latitude, position.Longitude);

            // --- Update tất cả shipments đang trên tàu ---
            var shipments = await _trainRepository.GetLoadedShipmentsByTrainAsync(trainId);
            foreach (var shipment in shipments)
            {
                await UpdateShipmentTracking(shipment, trainId, position);
            }
        }

        private async Task UpdateShipmentTracking(Shipment shipment, string trainId, TrainPositionResult position)
        {
            var rawTrainStatus = Enum.Parse<TrainStatusEnum>(position.Status);
            var mappedShipmentStatus = MapTrainStatusToShipmentStatus(rawTrainStatus);

            // Lấy current leg chưa hoàn tất
            var currentLeg = shipment.ShipmentItineraries
                .OrderBy(i => i.LegOrder)
                .FirstOrDefault(i => !i.IsCompleted);

            if (currentLeg != null && rawTrainStatus == TrainStatusEnum.ArrivedAtStation)
            {
                currentLeg.IsCompleted = true;
                currentLeg.Message = $"[Arrived at {currentLeg.Route?.ToStation?.StationNameVi} - {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}]";
                _shipmentItineraryRepository.Update(currentLeg);
            }

            // Cập nhật shipment status
            if (shipment.ShipmentStatus != mappedShipmentStatus)
            {
                shipment.ShipmentStatus = mappedShipmentStatus;

                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = mappedShipmentStatus,
                    Status = mappedShipmentStatus.ToString(),
                    EventTime = DateTimeOffset.UtcNow,
                    Note = $"Shipment moved to status: {mappedShipmentStatus}"
                });

                _shipmentRepository.Update(shipment);
            }

            // Nếu tất cả legs xong -> Delivered
            /*if (shipment.ShipmentItineraries.All(i => i.IsCompleted))
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.Delivered;

                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = ShipmentStatusEnum.Delivered,
                    Status = "Delivered",
                    EventTime = DateTimeOffset.UtcNow,
                    Note = "All legs completed. Shipment delivered."
                });

                _shipmentRepository.Update(shipment);
            }*/

            await _unitOfWork.SaveChangeAsync();

            // Đồng bộ Firebase shipment_tracking
            await _trainStateStore.SetShipmentTrackingAsync(shipment.TrackingCode!, trainId, position);

            _logger.Information("📦 Shipment {TrackingCode} updated → {Status}", shipment.TrackingCode, shipment.ShipmentStatus);
        }

        private ShipmentStatusEnum MapTrainStatusToShipmentStatus(TrainStatusEnum trainStatus)
        {
            return trainStatus switch
            {
                TrainStatusEnum.Departed => ShipmentStatusEnum.InTransit,
                TrainStatusEnum.InTransit => ShipmentStatusEnum.InTransit,
                TrainStatusEnum.ArrivedAtStation => ShipmentStatusEnum.AwaitingDelivery,
                _ => ShipmentStatusEnum.AwaitingDelivery
            };
        }
    }
}
