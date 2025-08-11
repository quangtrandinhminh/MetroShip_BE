using MetroShip.Service.Interfaces;
using MetroShip.Utility.Config;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;

namespace MetroShip.Service.Jobs;

public class ApplySurchargeJob(IServiceProvider serviceProvider) : IJob
{
    private readonly IShipmentService _shipmentService = serviceProvider.GetRequiredService<IShipmentService>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

    public async Task Execute(IJobExecutionContext context)
    {
        // Get the shipment ID from the job data map
        var shipmentId = context.JobDetail.JobDataMap.GetString("ApplySurcharge-for-shipmentId");
        if (string.IsNullOrEmpty(shipmentId))
        {
            throw new ArgumentException("Shipment ID is required for ApplySurchargeJob.");
        }

        try
        {
            await _shipmentService.ApplySurchargeForShipment(shipmentId);
            _logger.Information("Surcharge applied successfully for shipment ID: {ShipmentId}", shipmentId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to apply surcharge for shipment ID: {ShipmentId}", shipmentId);
            throw;
        }
    }
}