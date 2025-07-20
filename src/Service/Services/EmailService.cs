using System.Net;
using System.Net.Mail;
using MetroShip.Repository.Models;
using System.Text;
using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using Serilog;
using Serilog.Core;
using MetroShip.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using QRCoder;
using MetroShip.Service.BusinessModels;

namespace MetroShip.Service.Services
{
    public class EmailService(IServiceProvider serviceProvider) : IEmailService
    {
        private string EMAIL_SENDER = MailSettingModel.Instance.FromAddress;
        private string EMAIL_SENDER_PASSWORD = MailSettingModel.Instance.Smtp.Password;
        private string EMAIL_SENDER_HOST = MailSettingModel.Instance.Smtp.Host;
        private string EMAIL_SENDER_NAME = MailSettingModel.Instance.FromDisplayName;
        private int EMAIL_SENDER_PORT = Convert.ToInt16(MailSettingModel.Instance.Smtp.Port);
        private bool EMAIL_IsSSL = Convert.ToBoolean(MailSettingModel.Instance.Smtp.EnableSsl);
        private string APP_NAME = SystemSettingModel.Instance.ApplicationName;
        private ILogger _logger = Log.Logger;
        private readonly ISystemConfigRepository _systemConfigRepository = 
            serviceProvider.GetRequiredService<ISystemConfigRepository>();

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
                case MailTypeEnum.Notification:
                    CreateNotificationMail(model); 
                    break;
                default:
                    break;
            }
        }

        private void CreateVerifyMail(SendMailModel model)
        {
            try
            {
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(MailSettingModel.Instance.FromAddress, MailSettingModel.Instance.FromDisplayName),
                    Subject = $"{model.Token} là mã xác thực tài khoản {APP_NAME} của bạn"
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = $"Mã OTP của bạn là: {model.Token} " +
                               $"\nVui lòng nhập mã này để xác minh và hoàn tất việc tạo tài khoản.";

                SmtpClient smtp = new SmtpClient
                {
                    Host = EMAIL_SENDER_HOST,
                    Port = EMAIL_SENDER_PORT,
                    EnableSsl = EMAIL_IsSSL,
                    Credentials = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD)
                };

                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send email to {Email}", model.Email);
                //throw new AppException(ErrorCode.Unknown, $"Failed to send email: {ex.Message}");
            }
        }

        private void CreateResetPassMail(SendMailModel model)
        {
            try
            {
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(EMAIL_SENDER, EMAIL_SENDER_NAME),
                    Subject = $"Yêu cầu đặt lại mật khẩu cho tài khoản {model.Email} từ hệ thống {APP_NAME}"
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = $"OTP reset: {model.Token}";

                SmtpClient smtp = new SmtpClient
                {
                    Host = EMAIL_SENDER_HOST,
                    Port = EMAIL_SENDER_PORT,
                    EnableSsl = EMAIL_IsSSL,
                    Credentials = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD)
                };
                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send reset password email to {Email}", model.Email);
                //throw new AppException(ErrorCode.Unknown, $"Failed to send reset password email: {ex.Message}");
            }

        }

        private void CreateAccountMail(SendMailModel model)
        {
            try
            {
                var APP_NAME = SystemSettingModel.Instance.ApplicationName;
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(EMAIL_SENDER, EMAIL_SENDER_NAME),
                    Subject = $"Tài khoản {APP_NAME} của bạn đã được tạo thành công"
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = $"Tài khoản của bạn là: {model.UserName} " +
                               $"\nMật khẩu của bạn là: {model.Password} " +
                               $"\nVai trò của bạn là: {model.Role}" +
                               $"\nVui lòng đăng nhập và thay đổi mật khẩu.";

                SmtpClient smtp = new SmtpClient
                {
                    Host = EMAIL_SENDER_HOST,
                    Port = EMAIL_SENDER_PORT,
                    EnableSsl = EMAIL_IsSSL,
                    Credentials = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD)
                };

                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send email to {Email}", model.Email);
                //throw new AppException(ErrorCode.Unknown, $"Failed to send email: {ex.Message}");
            }
        }

        private void CreateOrderSuccessMail(SendMailModel model)
        {
            try
            {
                var shipment = model.Data as Shipment;
                if (shipment == null)
                {
                    throw new ArgumentException("Invalid shipment data for email");
                }

                var mailmsg = new MailMessage
                {
                    IsBodyHtml = true, // Changed to HTML for better formatting
                    From = new MailAddress(EMAIL_SENDER, EMAIL_SENDER_NAME),
                    Subject = $"Đơn hàng {shipment.TrackingCode} đã được đặt thành công trên {APP_NAME}"
                };
                mailmsg.To.Add(model.Email);

                // Create formatted email body instead of raw JSON
                mailmsg.Body = CreateShipmentEmailBody(shipment, model.Name, APP_NAME, model.Message);

                SmtpClient smtp = new SmtpClient
                {
                    Host = EMAIL_SENDER_HOST,
                    Port = EMAIL_SENDER_PORT,
                    EnableSsl = EMAIL_IsSSL,
                    Credentials = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD)
                };

                smtp.Send(mailmsg);

                _logger.Information("Email sent successfully to {Email} for shipment {TrackingCode}",
                    model.Email, shipment.TrackingCode);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send email to {Email}", model.Email);
                //throw new AppException(ErrorCode.Unknown, $"Failed to send email: {ex.Message}");
            }
        }

        private string CreateShipmentEmailBody(Shipment shipment, string customerName, string APP_NAME, string? trackingLink)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<html><body>");
            sb.AppendLine($"<h2>Chào {customerName},</h2>");
            sb.AppendLine($"<p>Cảm ơn bạn đã sử dụng dịch vụ vận chuyển của {APP_NAME}!</p>");
            sb.AppendLine($"<p>Đơn hàng của bạn đã được tiếp nhận thành công.</p>");

            sb.AppendLine($"<h3>Thông tin vận đơn:</h3>");
            sb.AppendLine($"<table border='1' cellpadding='5' cellspacing='0'>");
            sb.AppendLine($"<tr><td><strong>Mã vận đơn:</strong></td><td>{shipment.TrackingCode}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Người gửi:</strong></td><td>{shipment.SenderName}</td></tr>");
            sb.AppendLine($"<tr><td><strong>SĐT người gửi:</strong></td><td>{shipment.SenderPhone}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Người nhận:</strong></td><td>{shipment.RecipientName}</td></tr>");
            sb.AppendLine($"<tr><td><strong>SĐT người nhận:</strong></td><td>{shipment.RecipientPhone}</td></tr>");

            if (shipment.ScheduledDateTime.HasValue)
            {
                sb.AppendLine($"<tr><td><strong>Hạn chót gửi hàng tại trạm gửi:</strong></td><td>{shipment.ScheduledDateTime.Value:dd/MM/yyyy HH:mm}</td></tr>");
            }

            sb.AppendLine($"<tr><td><strong>Tổng phí vận chuyển:</strong></td><td>{shipment.TotalShippingFeeVnd:N0} VNĐ</td></tr>");
            sb.AppendLine($"<tr><td><strong>Tổng khoảng cách:</strong></td><td>{shipment.TotalKm:N2} km</td></tr>");

            if (shipment.TotalInsuranceFeeVnd.HasValue && shipment.TotalInsuranceFeeVnd > 0)
            {
                sb.AppendLine($"<tr><td><strong>Phí bảo hiểm:</strong></td><td>{shipment.TotalInsuranceFeeVnd:N0} VNĐ</td></tr>");
            }

            sb.AppendLine($"<tr><td><strong>Tổng cước:</strong></td><td>{shipment.TotalCostVnd:N0} VNĐ</td></tr>");
            sb.AppendLine($"</table>");

            // Add parcel information
            if (shipment.Parcels?.Any() == true)
            {
                sb.AppendLine($"<h3>Thông tin hàng hóa:</h3>");
                sb.AppendLine($"<table border='1' cellpadding='5' cellspacing='0'>");
                sb.AppendLine($"<tr><th>Mã kiện hàng</th><th>Kích thước (cm)</th><th>Khối lượng (kg)</th><th>Cước phí (VNĐ)</th></tr>");

                foreach (var parcel in shipment.Parcels)
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{parcel.ParcelCode}</td>");
                    sb.AppendLine($"<td>{parcel.LengthCm} x {parcel.WidthCm} x {parcel.HeightCm}</td>");
                    sb.AppendLine($"<td>{parcel.WeightKg}</td>");
                    sb.AppendLine($"<td>{parcel.PriceVnd:N0}</td>");
                    sb.AppendLine($"</tr>");
                }

                sb.AppendLine($"</table>");
            }

            sb.AppendLine($"<p>Vui lòng thanh toán trong 15 phút sau khi đặt đơn, nếu đã hoàn thành bạn có thể bỏ qua thông báo này</p>");
            sb.AppendLine($"<p>Bạn có thể theo dõi tình trạng vận đơn bằng cách nhập mã: <strong>{shipment.TrackingCode}</strong>" +
                $"<a href='{trackingLink ?? "#"}'>tại đây</a>" +
                $"</p>");
            sb.AppendLine($"<p>Hoặc quét mã QR bên dưới để theo dõi đơn hàng:</p>");
            // add qr code with tracking link
            if (trackingLink != null)
            {
                var qrCodeUrl = TrackingCodeGenerator.GenerateQRCode(trackingLink);
                sb.AppendLine($"<img src='{qrCodeUrl}' alt='QR Code for {shipment.TrackingCode}'/>");
            }

            /*sb.AppendLine($"<p>Đơn hàng của bạn sẽ được chúng tôi xác nhận trong <strong>" +
                $"{_systemConfigRepository.GetSystemConfigValueByKey(
                                       nameof(SystemConfigSetting.CONFIRMATION_HOUR)) ?? "24"}h</strong>!</p>");
            sb.AppendLine($"<p>Vui lòng theo dõi thông báo và thanh toán ngay sau khi đơn được xác nhận trong <strong>" +
                $"{_systemConfigRepository.GetSystemConfigValueByKey(
                    nameof(SystemConfigSetting.PAYMENT_REQUEST_HOUR)) ?? "24"}h</strong> kế tiếp!</p>");*/

            sb.AppendLine($"<p>Cảm ơn bạn đã tin tưởng sử dụng dịch vụ của chúng tôi!</p>");
            sb.AppendLine($"<p>Trân trọng,<br/>Đội ngũ {APP_NAME}</p>");
            sb.AppendLine($"</body></html>");

            return sb.ToString();
        }

        private void CreateNotificationMail(SendMailModel model)
        {
            try
            {
                var mailmsg = new MailMessage
                {
                    IsBodyHtml = false,
                    From = new MailAddress(EMAIL_SENDER, EMAIL_SENDER_NAME),
                    Subject = $"Thông báo từ {APP_NAME}"
                };
                mailmsg.To.Add(model.Email);

                mailmsg.Body = model.Message;

                SmtpClient smtp = new SmtpClient
                {
                    Host = EMAIL_SENDER_HOST,
                    Port = EMAIL_SENDER_PORT,
                    EnableSsl = EMAIL_IsSSL,
                    Credentials = new NetworkCredential(EMAIL_SENDER, EMAIL_SENDER_PASSWORD)
                };

                smtp.Send(mailmsg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send notification email to {Email}", 
                    model.Email);
                //throw new AppException(ErrorCode.Unknown, $"Failed to send notification email: {ex.Message}");
            }
        }
    }
}