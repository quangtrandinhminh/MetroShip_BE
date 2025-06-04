using System.Net;
using System.Net.Mail;
using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using Serilog;
using Serilog.Core;

namespace MetroShip.Service.Services
{
    public class EmailService(IServiceProvider serviceProvider) : IEmailService
    {
        private string EMAIL_SENDER = MailSettingModel.Instance.FromAddress;
        private string EMAIL_SENDER_PASSWORD = MailSettingModel.Instance.Smtp.Password;
        private string EMAIL_SENDER_HOST = MailSettingModel.Instance.Smtp.Host;
        private int EMAIL_SENDER_PORT = Convert.ToInt16(MailSettingModel.Instance.Smtp.Port);
        private bool EMAIL_IsSSL = Convert.ToBoolean(MailSettingModel.Instance.Smtp.EnableSsl);
        private ILogger _logger = Log.Logger;

        public void SendMail(SendMailModel model)
        {
            _logger.Information($"Send mail to {model.Email} with type {model.Type} and token {model.Token}");
            switch (model.Type)
            {
                case MailTypeEnum.Verify:
                    CreateVerifyMail(model);
                    break;
                case MailTypeEnum.ResetPassword:
                    CreateResetPassMail(model);
                    break;
                case MailTypeEnum.Account:
                    CreateAccountMail(model);
                    break;
                case MailTypeEnum.Shipment:
                    CreateOrderSuccessMail(model);
                    break;
                default:
                    break;
            }
        }

        private void CreateVerifyMail(SendMailModel model)
        {
            try
            {
                var appName = SystemSettingModel.Instance.ApplicationName;
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(MailSettingModel.Instance.FromAddress, MailSettingModel.Instance.FromDisplayName),
                    Subject = $"{model.Token} là mã xác thực tài khoản {appName} của bạn"
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = $"Mã OTP của bạn là: {model.Token} " +
                               $"\nVui lòng nhập mã này để xác minh và hoàn tất việc tạo tài khoản.";

                SmtpClient smtp = new SmtpClient();

                smtp.Host = EMAIL_SENDER_HOST;

                smtp.Port = EMAIL_SENDER_PORT;

                smtp.EnableSsl = EMAIL_IsSSL;

                var network = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD);
                smtp.Credentials = network;

                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCode.Unknown, ex.Message);
            }

        }

        private void CreateResetPassMail(SendMailModel model)
        {
            try
            {
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(MailSettingModel.Instance.FromAddress, MailSettingModel.Instance.FromDisplayName),
                    Subject = ""
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = $"OTP reset: {model.Token}";

                SmtpClient smtp = new SmtpClient();

                smtp.Host = EMAIL_SENDER_HOST;

                smtp.Port = EMAIL_SENDER_PORT;

                smtp.EnableSsl = EMAIL_IsSSL;
                var network = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD);
                smtp.Credentials = network;
                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCode.Unknown, ex.Message);
            }

        }

        private void CreateAccountMail(SendMailModel model)
        {
            try
            {
                var appName = SystemSettingModel.Instance.ApplicationName;
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(MailSettingModel.Instance.FromAddress, MailSettingModel.Instance.FromDisplayName),
                    Subject = $"Tài khoản {appName} của bạn đã được tạo thành công"
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = $"Tài khoản của bạn là: {model.UserName} " +
                               $"\nMật khẩu của bạn là: {model.Password} " +
                               $"\nVai trò của bạn là: {model.Role}" +
                               $"\nVui lòng đăng nhập và thay đổi mật khẩu.";

                SmtpClient smtp = new SmtpClient();

                smtp.Host = EMAIL_SENDER_HOST;

                smtp.Port = EMAIL_SENDER_PORT;

                smtp.EnableSsl = EMAIL_IsSSL;

                var network = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD);
                smtp.Credentials = network;

                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCode.Unknown, ex.Message);
            }
        }

        private void CreateOrderSuccessMail(SendMailModel model)
        {
            try
            {
                var JsonSerialize = new Newtonsoft.Json.JsonSerializer
                {
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.Indented
                };
                var data = Newtonsoft.Json.JsonConvert.SerializeObject(model.Data);

                var appName = SystemSettingModel.Instance.ApplicationName;
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(MailSettingModel.Instance.FromAddress, MailSettingModel.Instance.FromDisplayName),
                    Subject = $"Đặt hàng thành công trên {appName}"
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = $"Cảm ơn bạn đã đặt hàng trên {appName}. " +
                               $"\nThông tin đơn hàng: {data}";

                SmtpClient smtp = new SmtpClient();

                smtp.Host = EMAIL_SENDER_HOST;

                smtp.Port = EMAIL_SENDER_PORT;

                smtp.EnableSsl = EMAIL_IsSSL;

                var network = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD);
                smtp.Credentials = network;

                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCode.Unknown, ex.Message);
            }
        }
    }
}