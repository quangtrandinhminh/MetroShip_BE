using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System.Text.Json.Serialization;
using System.Text.Json;
using MetroShip.Repository.Models;

namespace MetroShip.Service.Jobs;

public class SendEmailJob(IServiceProvider serviceProvider) : IJob
{
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IEmailService _emailService = serviceProvider.GetRequiredService<IEmailService>();

    public Task Execute(IJobExecutionContext context)
    {
        // Deserialize from JSON
        var emailDataJson = context.JobDetail.JobDataMap.GetString("emailDataJson");

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        var sendMailModel = JsonSerializer.Deserialize<SendMailModel>(emailDataJson, options);
        if (sendMailModel.Data != null)
        {
            sendMailModel.Data = JsonSerializer.Deserialize<Shipment>(sendMailModel.Data.ToString(), options);
        }

        _logger.Information("Executing scheduled email job for: {Email}", sendMailModel.Email);

        try
        {
             _emailService.SendMail(sendMailModel); // Make this async if possible
            _logger.Information("Email sent successfully to: {Email} from Quartz", sendMailModel.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email to: {Email} from Quartz", sendMailModel.Email);
            throw; // Quartz will handle retries based on configuration
        }

        return Task.CompletedTask;
    }
}