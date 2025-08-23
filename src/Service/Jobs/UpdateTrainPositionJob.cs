using MetroShip.Repository.Interfaces;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Jobs
{
    public class UpdateTrainPositionJob(IServiceProvider serviceProvider) : IJob
    {
        private readonly ITrainService _trainService = serviceProvider.GetRequiredService<ITrainService>();
        private readonly ITrainStateStoreService _trainStateStore = serviceProvider.GetRequiredService<ITrainStateStoreService>();
        private readonly ITrainRepository _trainRepository = serviceProvider.GetRequiredService<ITrainRepository>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

        public async Task Execute(IJobExecutionContext context)
        {
            var trainId = context.JobDetail.JobDataMap.GetString("TrainId")!;
            var direction = await _trainStateStore.GetDirectionAsync(trainId);
            if (direction == null)
                return; // cannot proceed without direction

            var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction.Value);

            var segmentIndex = await _trainStateStore.GetSegmentIndexAsync(trainId);
            var startTime = await _trainStateStore.GetStartTimeAsync(trainId);

            if (segmentIndex == null || startTime == null)
                return; // không có state thì bỏ qua job

            var routes = train.Line.Routes
                .Where(r => r.Direction == direction)
                .OrderBy(r => r.SeqOrder)
                .ToList();

            // FIX: Map Route to RoutePolylineDto
            var routePolylines = routes
                .Select(r => new MetroShip.Service.ApiModels.Train.RoutePolylineDto
                {
                    FromStation = r.FromStationId,
                    ToStation = r.ToStationId,
                    SeqOrder = r.SeqOrder,
                    Direction = r.Direction,
                })
                .ToList();

            var position = TrainPositionCalculator.CalculatePosition(
                train,
                routePolylines,
                startTime.Value,
                segmentIndex.Value,
                direction.Value,
                train.TopSpeedKmH ?? 100  // Provide a default speed if TopSpeedKmH is null
            );

            await _trainStateStore.SetPositionResultAsync(trainId, position);
        }
    }
}
