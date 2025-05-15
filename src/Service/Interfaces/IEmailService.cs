using MetroShip.Service.ApiModels;

namespace MetroShip.Service.Interfaces
{
    public interface IEmailService
    {
        void SendMail(SendMailModel model);
    }
}