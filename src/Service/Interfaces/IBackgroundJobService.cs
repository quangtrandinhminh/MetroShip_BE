namespace MetroShip.Service.Interfaces;

public interface IBackgroundJobService
{
    Task ScheduleUpdateNoDropOffJob(string shipmentId, DateTimeOffset scheduledDateTime);
    Task CancelUpdateNoDropOffJob(string shipmentId);
    Task ScheduleUnpaidJob(string shipmentId, DateTimeOffset paymentDeadline);
    Task CancelScheduledUnpaidJob(string shipmentId);
    Task ScheduleCancelTransactionJob(string transactionId, DateTimeOffset paymentDeadline);
    Task CancelScheduleCancelTransactionJob(string transactionId);
    Task ScheduleApplySurchargeJob(string shipmentId, string pricingConfigId);
    Task CancelScheduleApplySurchargeJob(string shipmentId);
    Task CancelScheduleSimulateTrainJob(string trainId);
}