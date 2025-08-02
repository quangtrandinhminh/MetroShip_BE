using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;

namespace MetroShip.Service.Jobs;

public class SendEmailJob(IServiceProvider serviceProvider) : IJob
{
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IEmailService _emailService = serviceProvider.GetRequiredService<IEmailService>();

    public Task Execute(IJobExecutionContext context)
    {
        // Deserialize from JSON
        var emailDataJson = context.JobDetail.JobDataMap.GetString("emailDataJson");
        var sendMailModel = System.Text.Json.JsonSerializer.Deserialize<SendMailModel>(emailDataJson);

        _logger.Information("Executing scheduled email job for: {Email}", sendMailModel.Email);

        try
        {
             _emailService.SendMail(sendMailModel); // Make this async if possible
            _logger.Information("Email sent successfully to: {Email}", sendMailModel.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email to: {Email}", sendMailModel.Email);
            throw; // Quartz will handle retries based on configuration
        }

        return Task.CompletedTask;
    }
}