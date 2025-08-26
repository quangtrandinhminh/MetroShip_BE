using MetroShip.Service.Interfaces;
using MetroShip.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using ILogger = Serilog.ILogger;

namespace MetroShip.WebAPI.Jobs;

public class SimulateTrainJob(IServiceProvider serviceProvider) : IJob
{
    private readonly ITrainService _trainService = serviceProvider.GetRequiredService<ITrainService>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IHubContext<TrackingHub> _hub = serviceProvider.GetRequiredService<IHubContext<TrackingHub>>();

    public async Task Execute(IJobExecutionContext context)
    {
        // Get the train ID from the job data map
        var trainId = context.JobDetail.JobDataMap.GetString("Simulate-for-trainId");
        if (string.IsNullOrEmpty(trainId))
        {
            throw new ArgumentException("Train ID is required for SimulateTrainJob.");
        }

        _logger.Information("Starting simulation for train ID: {TrainId}", trainId);
        try
        {
            var response = await _trainService.GetTrainPositionAsync(trainId);
            await _hub.Clients.Group(trainId).SendAsync("ReceiveLocationUpdate", response);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to simulate train for train ID: {TrainId}", trainId);
            throw; // Quartz will handle retries based on configuration
        }
    }
}