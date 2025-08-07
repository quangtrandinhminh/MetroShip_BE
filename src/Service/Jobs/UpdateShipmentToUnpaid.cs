using MetroShip.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;

namespace MetroShip.Service.Jobs;

public class UpdateShipmentToUnpaid(IServiceProvider serviceProvider) : IJob
{
    private readonly IShipmentService _shipmentService = serviceProvider.GetRequiredService<IShipmentService>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    public async Task Execute(IJobExecutionContext context)
    {
        // Get the shipment ID from the job data map
        var shipmentId = context.JobDetail.JobDataMap.GetString("Unpaid-for-shipmentId");
        if (string.IsNullOrEmpty(shipmentId))
        {
            throw new ArgumentException("Shipment ID is required for UpdateShipmentToUnpaid job.");
        }

        await _shipmentService.UpdateShipmentStatusUnpaid(shipmentId);
    }
}