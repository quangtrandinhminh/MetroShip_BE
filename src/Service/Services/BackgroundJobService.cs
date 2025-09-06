using MetroShip.Service.Interfaces;
using MetroShip.Service.Jobs;
using MetroShip.Utility.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;

namespace MetroShip.Service.Services;

public class BackgroundJobService(IServiceProvider serviceProvider) : IBackgroundJobService
{
    private readonly ISchedulerFactory _schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();

    public async Task ScheduleUpdateNoDropOffJob(string shipmentId, DateTimeOffset scheduledDateTime)
    {
        _logger.Information("Scheduling job to update shipment status to NoDropOff for ID: {@shipmentId}", shipmentId);
        var jobData = new JobDataMap
        {
            { "NoDropOff-for-shipmentId", shipmentId }
        };

        // Schedule the job to run after 15 minutes
        var jobDetail = JobBuilder.Create<UpdateShipmentToNoDropOff>()
            .WithIdentity($"UpdateShipmentToNoDropOff-{shipmentId}")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Trigger-UpdateShipmentToNoDropOff-{shipmentId}")
            .StartAt(scheduledDateTime.AddMinutes(5))
            //.StartAt(DateTimeOffset.UtcNow.AddSeconds(5))
            .Build();

        await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
    }

    public async Task CancelUpdateNoDropOffJob(string shipmentId)
    {
        _logger.Information("Cancelling job to update shipment status to NoDropOff for ID: {@shipmentId}", shipmentId);
        var jobKey = new JobKey($"UpdateShipmentToNoDropOff-{shipmentId}");
        var scheduler = await _schedulerFactory.GetScheduler();
        if (await scheduler.CheckExists(jobKey))
        {
            await scheduler.DeleteJob(jobKey);
            _logger.Information("Cancelled job for shipment ID: {@shipmentId}", shipmentId);
        }
        else
        {
            _logger.Warning("No scheduled job found for shipment ID: {@shipmentId}", shipmentId);
        }
    }

    public async Task ScheduleUnpaidJob(string shipmentId, DateTimeOffset paymentDeadline)
    {
        _logger.Information("Scheduling job to update shipment status to unpaid for ID: {@shipmentId}", shipmentId);
        var jobData = new JobDataMap
        {
            { "Unpaid-for-shipmentId", shipmentId }
        };

        // Schedule the job to run after 15 minutes
        var jobDetail = JobBuilder.Create<UpdateShipmentToUnpaid>()
            .WithIdentity($"UpdateShipmentToUnpaid-{shipmentId}")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Trigger-UpdateShipmentToUnpaid-{shipmentId}")
            .StartAt(paymentDeadline)
            //.StartAt(DateTimeOffset.UtcNow.AddSeconds(5))
            .Build();

        await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
    }

    // cancel ScheduleUnpaidJob
    public async Task CancelScheduledUnpaidJob(string shipmentId)
    {
        _logger.Information("Cancelling scheduled job to update shipment status to unpaid for ID: {@shipmentId}", shipmentId);
        var jobKey = new JobKey($"UpdateShipmentToUnpaid-{shipmentId}");
        var scheduler = await _schedulerFactory.GetScheduler();
        if (await scheduler.CheckExists(jobKey))
        {
            await scheduler.DeleteJob(jobKey);
            _logger.Information("Cancelled scheduled job for shipment ID: {@shipmentId}", shipmentId);
        }
        else
        {
            _logger.Warning("No scheduled job found for shipment ID: {@shipmentId}", shipmentId);
        }
    }

    // schedule job to cancel transaction after 15 minutes
    public async Task ScheduleCancelTransactionJob(string transactionId, DateTimeOffset paymentDeadline)
    {
        _logger.Information("Scheduling job to cancel transaction with ID: {transactionId}", transactionId);
        var jobData = new JobDataMap
        {
            { "CancelTransaction-for-transactionId", transactionId }
        };

        // Schedule the job to run after 15 minutes
        var jobDetail = JobBuilder.Create<CancelTransactionJob>()
            .WithIdentity($"CancelTransaction-{transactionId}")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Trigger-CancelTransaction-{transactionId}")
            .StartAt(paymentDeadline)
            .Build();

        await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
    }

    // cancel transaction cancellation job by transactionId
    public async Task CancelScheduleCancelTransactionJob(string transactionId)
    {
        _logger.Information("Cancelling scheduled job to cancel transaction with ID: {transactionId}", transactionId);
        var jobKey = new JobKey($"CancelTransaction-{transactionId}");
        var scheduler = await _schedulerFactory.GetScheduler();
        if (await scheduler.CheckExists(jobKey))
        {
            await scheduler.DeleteJob(jobKey);
            _logger.Information("Cancelled scheduled job for transaction ID: {transactionId}", transactionId);
        }
        else
        {
            _logger.Warning("No scheduled job found for transaction ID: {transactionId}", transactionId);
        }
    }

    public async Task ScheduleApplySurchargeJob(string shipmentId, string pricingConfigId)
    {
        _logger.Information("Scheduling job to apply surcharge for shipment ID: {@shipmentId}", shipmentId);
        var jobData = new JobDataMap
        {
            { "ApplySurcharge-for-shipmentId", shipmentId }
        };

        var freeStoreDays = await _pricingService.GetFreeStoreDaysAsync(pricingConfigId);

        // Schedule the job to run after 15 minutes
        var jobDetail = JobBuilder.Create<ApplySurchargeJob>()
            .WithIdentity($"ApplySurchargeJob-{shipmentId}")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Trigger-ApplySurchargeJob-{shipmentId}")
            .StartAt(DateTimeOffset.UtcNow.AddDays(freeStoreDays))
            //.StartAt(DateTimeOffset.UtcNow.AddSeconds(5))
            // Repeat every 24 hours
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(24)
                .RepeatForever())
            .Build();

        await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
    }
    // cancel apply surcharge job
    public async Task CancelScheduleApplySurchargeJob(string shipmentId)
    {
        _logger.Information("Canceling apply surcharge job for shipment ID: {@shipmentId}", shipmentId);
        var jobKey = new JobKey($"ApplySurchargeJob-{shipmentId}");
        if (await _schedulerFactory.GetScheduler().Result.CheckExists(jobKey))
        {
            await _schedulerFactory.GetScheduler().Result.DeleteJob(jobKey);
            _logger.Information("Apply surcharge job canceled for shipment ID: {@shipmentId}", shipmentId);
        }
        else
        {
            _logger.Warning("No apply surcharge job found for shipment ID: {@shipmentId}", shipmentId);
        }
    }

    // cancel simulate train job when train is stopped or completed
    public async Task CancelScheduleSimulateTrainJob(string trainId)
    {
        _logger.Information("Cancelling simulator train job for train ID: {TrainId}", trainId);
        var jobKey = new JobKey($"SimulatorTrainJob-{trainId}");
        var scheduler = await _schedulerFactory.GetScheduler();
        if (await scheduler.CheckExists(jobKey))
        {
            await scheduler.DeleteJob(jobKey);
            _logger.Information("Cancelled simulator train job for train ID: {TrainId}", trainId);
        }
        else
        {
            _logger.Warning("No scheduled simulator job found for train ID: {TrainId}", trainId);
        }
    }
}