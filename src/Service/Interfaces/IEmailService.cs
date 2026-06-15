using MetroShip.Service.ApiModels;

namespace MetroShip.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendMail(SendMailModel model);
        Task ScheduleEmailJob(SendMailModel emailData);
    }
}