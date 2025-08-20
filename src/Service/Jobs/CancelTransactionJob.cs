using MetroShip.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace MetroShip.Service.Jobs;

public class CancelTransactionJob(IServiceProvider serviceProvider) : IJob
{
    private readonly ITransactionService _transactionService = serviceProvider.GetRequiredService<ITransactionService>();

    public async Task Execute(IJobExecutionContext context)
    {
        var transactionId = context.JobDetail.JobDataMap.GetString("CancelTransaction-for-transactionId");
        if (string.IsNullOrEmpty(transactionId))
        {
            throw new ArgumentException("TransactionId is required for CancelTransactionJob.");
        }

        // Call the service to cancel the transaction
        await _transactionService.CancelTransactionAsync(transactionId);
    }
}